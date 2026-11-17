using ServiceManager.ConfigBase;
using System;
using System.Collections.Generic;

namespace ServiceManager.UI.Config
{
    /// <summary>
    /// How the window looks on this machine. This is not the settings of a ServiceManager instance, it belongs
    /// to the person sitting in front of the screen, so it sits in the local application data instead of the
    /// config folder of an instance and is never sent over the wire.
    /// </summary>
    public class UiConfig : ConfigBase<UiConfig>
    {
        Dictionary<String, Boolean> m_Columns = new Dictionary<String, Boolean>();

        /// <summary>
        /// The columns a person switched on or off, kept under the name of the column. A column that is not in
        /// here follows the default of its definition.
        /// </summary>
        public Dictionary<String, Boolean> Columns
        {
            get { return m_Columns; }
            set { m_Columns = value ?? new Dictionary<String, Boolean>(); }
        }

        /// <summary>
        /// The file of this machine. Every person on the machine has one of their own.
        /// </summary>
        public static String FilePath
        {
            get
            {
                return System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ServiceManager",
                    "ui.cfg");
            }
        }

        /// <summary>
        /// Reads the file of this machine. A missing file means everything is on its default, a broken file
        /// is not worth a message: the window simply starts with the defaults.
        /// </summary>
        public static UiConfig Load()
        {
            try
            {
                return load(FilePath);
            }
            catch (Exception)
            {
                return new UiConfig();
            }
        }

        /// <summary>
        /// Writes the file of this machine. A machine that cannot be written keeps the choice for this run.
        /// </summary>
        public void Store()
        {
            save(FilePath);
        }
    }
}
