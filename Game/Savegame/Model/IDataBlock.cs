namespace Playblack.Savegame.Model {

    /// <summary>
    /// Common interface to mark things as DataBlock in a save file.
    /// </summary>
    public interface IDataBlock {

        /// <summary>
        /// Defines under what name this data block should be stored.
        /// </summary>
        /// <value>The data identifier.</value>
        string DataId {
            get;
        }
    }
}