using System;
using UnityEngine;

// https://answers.unity.com/questions/487121/automatically-assigning-gameobjects-a-unique-and-c.html
namespace Playblack.Savegame {

    public class UniqueIdentifierAttribute : PropertyAttribute {
    }

    public class UniqueId : MonoBehaviour {
        [SerializeField]
        [UniqueIdentifier]
        private string uniqueId;

        public string UId {
            get {
                return uniqueId;
            }
        }

        /// <summary>
        /// Creates and possibly overrides a UID.
        /// USE WITH EXTREME CAUTION.
        /// In regular day to day use, you will NOT need this.
        /// At all. Beware! It might break your save games!
        /// </summary>
        public void CreateUid() {
            Guid guid = Guid.NewGuid();
            uniqueId = guid.ToString();
        }
    }
}
