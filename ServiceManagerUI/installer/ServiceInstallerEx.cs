/////////////////////////////////////////////////////////////////////////////
//
// Library Name		: Verifide.ServiceUtils.dll
// Description		: Extension to System.ServiceProcess.ServiceInstallerEx
//					  to enable configuration of advanced options
// Date				: 1/14/04
// Url				: https://www.codeproject.com/Articles/6164/A-ServiceInstaller-Extension-That-Enables-Recovery
//
//	Copyright (C) 2004 Narendra (Neil) Baliga
//
//	This library is free software; you can redistribute it and/or
//	modify it as you wish. It is distributed in the hope that it will 
//	be useful,but WITHOUT ANY WARRANTY; without even the implied 
//	warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
//
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Configuration.Install;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Collections;
using System.Diagnostics;

namespace Verifide.ServiceUtils
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class ServiceInstallerEx : System.ServiceProcess.ServiceInstaller
	{

		#region Misc Win32 Interop Stuff

		// The struct for setting the service description
		[StructLayout(LayoutKind.Sequential)]
			public struct SERVICE_DESCRIPTION {

			public string lpDescription;

		}

		// The struct for setting the service failure actions
		[StructLayout(LayoutKind.Sequential)]
			public struct SERVICE_FAILURE_ACTIONS {

			public int		dwResetPeriod;
			public string	lpRebootMsg;
			public string	lpCommand;
			public int		cActions;
			public int		lpsaActions;

		}

		// Win32 function to open the service control manager
		[DllImport("advapi32.dll")]
		public static extern 
			IntPtr OpenSCManager(string lpMachineName, string lpDatabaseName, int dwDesiredAccess);

		// Win32 function to open a service instance
		[DllImport("advapi32.dll")]
		public static extern IntPtr 
			OpenService( IntPtr hSCManager, string lpServiceName, int dwDesiredAccess);

		// Win32 function to lock the service database to perform write operations.
		[DllImport("advapi32.dll")]
		public static extern IntPtr 
			LockServiceDatabase(IntPtr hSCManager);

		// Win32 function to change the service config for the failure actions.
		[DllImport("advapi32.dll", EntryPoint="ChangeServiceConfig2")]
		public static extern bool 
			ChangeServiceFailureActions(  IntPtr hService, int dwInfoLevel, 
			[ MarshalAs( UnmanagedType.Struct ) ] ref SERVICE_FAILURE_ACTIONS lpInfo );

		// Win32 function to change the service config for the service description
		[DllImport("advapi32.dll", EntryPoint="ChangeServiceConfig2")]
		public static extern bool 
			ChangeServiceDescription(  IntPtr hService, int dwInfoLevel, 
			[ MarshalAs( UnmanagedType.Struct ) ] ref SERVICE_DESCRIPTION lpInfo );

		// Win32 function to close a service related handle.
		[DllImport("advapi32.dll")]
		public static extern bool 
			CloseServiceHandle( IntPtr hSCObject);
	
		// Win32 function to unlock the service database.
		[DllImport("advapi32.dll")]
		public static extern bool 
			UnlockServiceDatabase(IntPtr hSCManager);

		// The infamous GetLastError() we have all grown to love
		[DllImport("kernel32.dll")]
		public static extern int 
			GetLastError();

		// Some Win32 constants I'm using in this app

		private const int SC_MANAGER_ALL_ACCESS				= 0xF003F;
		private const int SERVICE_ALL_ACCESS				= 0xF01FF;
		private const int SERVICE_CONFIG_DESCRIPTION		= 0x1;
		private const int SERVICE_CONFIG_FAILURE_ACTIONS	= 0x2;
		private const int SERVICE_NO_CHANGE					= -1;
		private const int ERROR_ACCESS_DENIED				= 5;


		#endregion

		#region Shutdown Privilege Interop Stuff

		// Struct required to set shutdown privileges
		[StructLayout(LayoutKind.Sequential)]
			public struct LUID_AND_ATTRIBUTES{

			public long Luid;
			public int  Attributes;

		}

		// Struct required to set shutdown privileges. The Pack attribute specified here
		// is important. We are in essence cheating here because the Privileges field is
		// actually a variable size array of structs.  We use the Pack=1 to align the Privileges
		// field exactly after the PrivilegeCount field when marshalling this struct to
		// Win32. You do not want to know how many hours I had to spend on this alone!!!

		[StructLayout(LayoutKind.Sequential, Pack=1)]
			public struct TOKEN_PRIVILEGES{

			public int     PrivilegeCount;
			public LUID_AND_ATTRIBUTES Privileges;

		}

		// This method adjusts privileges for this process which is needed when
		// specifying the reboot option for a service failure recover action.
		[DllImport("advapi32.dll")]
		public static extern bool
			AdjustTokenPrivileges( IntPtr TokenHandle, bool DisableAllPrivileges, 
			[ MarshalAs( UnmanagedType.Struct ) ] ref TOKEN_PRIVILEGES NewState, int BufferLength,
			IntPtr PreviousState, ref int ReturnLength );


		// Looks up the privilege code for the privilege name
		[DllImport("advapi32.dll")]
		public static extern bool
			LookupPrivilegeValue( string lpSystemName, string lpName, ref long lpLuid );

		// Opens the security/privilege token for a process handle
		[DllImport("advapi32.dll")]
		public static extern bool
			OpenProcessToken( IntPtr ProcessHandle, int DesiredAccess, ref IntPtr TokenHandle );

		// Gets the current process handle
		[DllImport("kernel32.dll")]
		public static extern IntPtr
			GetCurrentProcess();

		// Close a windows handle
		[DllImport("kernel32.dll")]
		public static extern bool 
			CloseHandle( IntPtr hndl );

		// Token adjustment stuff
		private const int TOKEN_ADJUST_PRIVILEGES			= 32;
		private const int TOKEN_QUERY						= 8;
		private const string SE_SHUTDOWN_NAME				= "SeShutdownPrivilege";
		private const int SE_PRIVILEGE_ENABLED				= 2;

		#endregion

		private string description="";						// description
		private int    failResetTime=SERVICE_NO_CHANGE;		// fail count reset time
		private string failRebootMsg="";					// reboot msg
		private string failRunCommand="";					// fail run command
		private bool   setDescription = false;				// flag 
		private bool   setFailActions = false;
		private bool   startOnInstall = false;
		private int    startTimeout   = 15000;

		private string logMsgBase;

		// List of Failure Actions
		public ArrayList FailureActions;

		// Constructor: Init the failure actions and register for the Committed event
		public ServiceInstallerEx() : base()
		{

			FailureActions = new ArrayList();

			// Multicast delegates are cool. By registering and activating here, we
			// shield user from having to deal with the event handlers. They simply
			// set the properties and we do the work for them.

			// Register the event handlers for post install operations
			base.Committed += new InstallEventHandler( this.UpdateServiceConfig );
			base.Committed += new InstallEventHandler( this.StartIfNeeded );

			// Set the Log Msg Base
			logMsgBase = "ServiceInstallerEx : " + base.ServiceName + " : ";

		}

		#region Class Properties

		// Property for setting the service description
		public string Description{ 
			set{
				description = value;
				setDescription = true;
			}
		}

		// Property to set fail count reset time
		public int FailCountResetTime {
			set{ 
				failResetTime = value; 
				setFailActions = true;
			}
		}

		// Property to set fail reboot msg
		public string FailRebootMsg{
			set{ 
				failRebootMsg = value; 
				setFailActions = true;
			}
		}

		// Property to set fail run command.
		public string FailRunCommand{
			set{ 
				failRunCommand = value; 
				setFailActions = true;
			}
		}

		// Property style access to configure the service to start on install
		public bool StartOnInstall {
			set{
				this.startOnInstall = value;
			}
		}

		// Property to set the start timeout for the service.
		public int StartTimeout{
			set{
				this.startTimeout = value;
			}
		}

		#endregion


		///////////////////////////////////////////////////////////////////////////////////
		// Method to log to console and event log

		private void LogInstallMessage( EventLogEntryType logLevel , string msg ){

			Console.WriteLine( msg );

			try {
				EventLog.WriteEntry( base.ServiceName, msg, logLevel );
			}
			catch(Exception ex ){
				Console.WriteLine( ex.ToString());
			}

		}

		private bool GrandShutdownPrivilege(){

			// This code mimics the MSDN defined way to adjust privilege for shutdown
			// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/sysinfo/base/shutting_down.asp

			bool retRslt = false;

			IntPtr hToken = IntPtr.Zero ;
			IntPtr myProc = IntPtr.Zero;

			TOKEN_PRIVILEGES tkp = new TOKEN_PRIVILEGES();

			long Luid = 0;
			int  retLen = 0;

			try {

				myProc = GetCurrentProcess();
				bool rslt = OpenProcessToken( myProc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref hToken );
				if( !rslt ) return retRslt;

				LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, ref Luid );

				tkp.PrivilegeCount = 1;
				tkp.Privileges.Luid = Luid;
				tkp.Privileges.Attributes = SE_PRIVILEGE_ENABLED;

				rslt = AdjustTokenPrivileges( hToken, false, ref tkp, 0, IntPtr.Zero , ref retLen );

				if( GetLastError() != 0 ){

					throw new Exception( "Failed to grant shutdown privilege" );

				}

				retRslt = true;

			}
			catch( Exception ex ){

				LogInstallMessage( EventLogEntryType.Error , logMsgBase + ex.Message );

			}
			finally{

				if( hToken != IntPtr.Zero ){

					CloseHandle( hToken );

				}
			}

			return retRslt;

		}

		////////////////////////////////////////////////////////////////////////////////////
		// The worker method to set all the extension properties for the service

		public void UpdateServiceConfig( object sender, InstallEventArgs e ){

			// Determine if we need to set fail actions

			this.setFailActions = false;

			int numActions = FailureActions.Count;

			if( numActions > 0 ) {
				setFailActions = true;
			}

			// Do we need to do any work that the base installer did not do already?
			if( !(this.setDescription || this.setFailActions  )  ) return;

			// We've got work to do
			IntPtr scmHndl = IntPtr.Zero;
			IntPtr svcHndl = IntPtr.Zero;
			IntPtr tmpBuf  = IntPtr.Zero;
			IntPtr svcLock = IntPtr.Zero;

			// Err check var
			bool rslt = false;


			// Place all our code in a try block
			try {

				// Open the service control manager
				scmHndl = OpenSCManager( null, null, SC_MANAGER_ALL_ACCESS );

				if( scmHndl.ToInt32() <= 0 ) {
					LogInstallMessage( EventLogEntryType.Error, logMsgBase + "Failed to Open Service Control Manager" );
					return;
				}

				// Lock the Service Database
				svcLock = LockServiceDatabase( scmHndl );

				if( svcLock.ToInt32() <=0 ){

					LogInstallMessage( EventLogEntryType.Error, logMsgBase + "Failed to Lock Service Database for Write" ) ;
					return;
				}

				// Open the service
				svcHndl = OpenService( scmHndl, base.ServiceName, SERVICE_ALL_ACCESS );

				if( svcHndl.ToInt32() <= 0 ){

					LogInstallMessage( EventLogEntryType.Information,  logMsgBase + "Failed to Open Service "  );
					return;
				}

				// Need to set service failure actions. Note that the API lets us set as many as
				// we want, yet the Service Control Manager GUI only lets us see the first 3.
				// Bill is aware of this and has promised no fixes. Also note that the API allows
				// granularity of seconds whereas GUI only shows days and minutes.

				if( this.setFailActions ) {
				
					// We're gonna serialize the SA_ACTION structs into an array of ints
					// for simplicity in marshalling this variable length ptr to win32

					int[] actions = new int[numActions*2];
					
					int currInd = 0;

					bool needShutdownPrivilege = false;

					foreach( FailureAction fa in FailureActions ){

						actions[currInd]	= (int)fa.Type;
						actions[++currInd]	= fa.Delay;
						currInd++;

						if( fa.Type == RecoverAction.Reboot ) {
							needShutdownPrivilege = true;
						}
												 
					}

					// If we need shutdown privilege, then grant it to this process
					if( needShutdownPrivilege ){

						rslt = this.GrandShutdownPrivilege();

						if( !rslt ) return;

					}

					// Need to pack 8 bytes per struct
					tmpBuf = Marshal.AllocHGlobal( numActions*8 );

					// Move array into marshallable pointer
					Marshal.Copy( actions, 0, tmpBuf, numActions*2 );

					// Set the SERVICE_FAILURE_ACTIONS struct
					SERVICE_FAILURE_ACTIONS sfa = new SERVICE_FAILURE_ACTIONS();

					sfa.cActions = numActions ;
					sfa.dwResetPeriod = this.failResetTime;
					sfa.lpCommand = this.failRunCommand;
					sfa.lpRebootMsg = this.failRebootMsg;
					sfa.lpsaActions = tmpBuf.ToInt32();


					// Call the ChangeServiceFailureActions() abstraction of ChangeServiceConfig2()
					rslt = ChangeServiceFailureActions( svcHndl, SERVICE_CONFIG_FAILURE_ACTIONS, ref sfa );

					//Check the return
					if( !rslt ){

						int err = GetLastError();

						if( err == ERROR_ACCESS_DENIED ){
							throw new Exception( logMsgBase + "Access Denied while setting Failure Actions" );
						}

					}

					// Free the memory
					Marshal.FreeHGlobal( tmpBuf ); tmpBuf = IntPtr.Zero;

					LogInstallMessage( EventLogEntryType.Information,  logMsgBase + "Successfully configured Failure Actions" );

				}

				// Need to set the description field?
				if( this.setDescription ){
	
					SERVICE_DESCRIPTION sd = new SERVICE_DESCRIPTION();
					sd.lpDescription = this.description;

					// Call the ChangeServiceDescription() abstraction of ChangeServiceConfig2()
					rslt = ChangeServiceDescription( svcHndl, SERVICE_CONFIG_DESCRIPTION, ref sd );

					// Error setting description?
					if( !rslt ){

						throw new Exception( logMsgBase + "Failed to set description" );

					}

					LogInstallMessage( EventLogEntryType.Information ,  logMsgBase + "Successfully set description" );

				}

			}
				// Catch all exceptions
			catch( Exception ex ){

				LogInstallMessage( EventLogEntryType.Error,  ex.Message  );

			}
			finally{

				if( scmHndl != IntPtr.Zero ){
					
					// Unlock the service database
					if( svcLock != IntPtr.Zero ){
						UnlockServiceDatabase( svcLock );
						svcLock = IntPtr.Zero;
					}

					// Close the service control manager handle
					CloseServiceHandle( scmHndl );
					scmHndl = IntPtr.Zero;
				}

				// Close the service handle
				if( svcHndl != IntPtr.Zero ){
					CloseServiceHandle(svcHndl);
					svcHndl = IntPtr.Zero;
				}

				// Free the memory
				if( tmpBuf != IntPtr.Zero ){
					Marshal.FreeHGlobal( tmpBuf );
					tmpBuf = IntPtr.Zero;
				}
				

			} // End try-catch-finally

		}


		
		////////////////////////////////////////////////////////////////////////////////////
		// Method to start the service automatically after installation

		private void StartIfNeeded( object sender, InstallEventArgs e ){

			// Do we need to do any work?
			if( !this.startOnInstall ) return;

			try{

				TimeSpan waitTo = new TimeSpan( 0,0, this.startTimeout );

				// Get a handle to the service
				ServiceController sc = new ServiceController( base.ServiceName );
			
				// Start the service and wait for it to start
				sc.Start();
				sc.WaitForStatus( ServiceControllerStatus.Running, waitTo );

				// Be good and release our handle
				sc.Close();

				LogInstallMessage( EventLogEntryType.Information , logMsgBase + " Service Started" ) ;

			}
				// Catch all exceptions
			catch( Exception ex ){

				LogInstallMessage( EventLogEntryType.Error, logMsgBase + ex.Message );

			}

		}

	}

	// Enum for recovery actions (correspond to the Win32 equivalents )
	public enum RecoverAction{

		None=0, Restart=1, Reboot=2, RunCommand=3

	}
	
	// Class to represent a failure action which consists of a recovery
	// action type and an action delay
	public class FailureAction {

		private RecoverAction type = RecoverAction.None;
		private int delay=0;

		// Default constructor
		public FailureAction(){

		}

		// Constructor
		public FailureAction( RecoverAction actionType, int actionDelay ){

			this.type = actionType;
			this.delay = actionDelay;

		}

		// Property to set recover action type
		public RecoverAction Type{

			get{ return type; }

			set{
				type = value;
			}

		}
        
		// Property to set recover action delay
		public int Delay{

			get{ return delay; }

			set{
				delay = value;
			}
				   
		}
			
	}

}
