namespace Playblack.Savegame {

    public enum SaveField {
        /// <summary>
        /// Decorate primitives or arrays of primitives with this.
        /// that is: int, float, double, string, bool.
        /// </summary>
        FIELD_PRIMITIVE = 10,
        /// <summary>
        /// Decorate Unity Color fields with this.
        /// </summary>
        FIELD_COLOR,
        /// <summary>
        /// Decorate Unity Vector3 fields with this
        /// </summary>
        FIELD_VECTOR_3,
        /// <summary>
        /// Decorate Unity Vector2 fields with this
        /// </summary>
        FIELD_VECTOR_2,
        /// <summary>
        /// Decorate Unity Quaternion fields with this
        /// </summary>
        FIELD_QUATERNION,
        /// <summary>
        /// Decorate fields with this that should be serialized by Protobuf.
        /// These can be scalar or array/list values.
        /// </summary>
        FIELD_PROTOBUF_OBJECT,
        /// <summary>
        /// Decorate fields with this that are simple serializable classes.
        /// This will only serialize the fields inside the object that are decorated with SaveableField!
        /// </summary>
        FIELD_SIMPLE_OBJECT
    }
}