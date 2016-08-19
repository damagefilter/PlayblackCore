namespace Playblack.Savegame {

    public enum SaveField {
        // Primitives can be scalar or array types.
        // Immediately restorable
        FIELD_FLOAT = 0,
        FIELD_INT,
        FIELD_STRING,
        FIELD_BOOL,
        // These can only be scalar objects as they need some special serialisation
        // as they are Unity types and are not marked serializable
        FIELD_COLOR,
        FIELD_VECTOR_POSITION, // FIXME: Rename to just VECTOR at some point
        FIELD_QUATERNION,
        // These can be scalar or array types again.
        FIELD_PROTOBUF_OBJECT, // defines a field with an protocontract annotated object type
        FIELD_SIMPLE_OBJECT// defines a simple savebale object (class with some values in it using SaveableField annotations)
    }
}