using System;

namespace Playblack.BehaviourTree {

    [Serializable]
    public enum ValueType {
        FLOAT,
        INT,
        STRING, // one-liners
        TEXT, // long texts
        BOOL,
        ENUM
    }
}
