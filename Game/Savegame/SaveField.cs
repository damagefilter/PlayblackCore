namespace Playblack.Savegame {

    public enum SaveField {

        // Immediately restorable
        FIELD_FLOAT = 0,

        FIELD_INT,
        FIELD_STRING,
        FIELD_BOOL,
        FIELD_COLOR,
        FIELD_VECTOR_POSITION,
        FIELD_QUATERNION,
        FIELD_PROTOBUF_OBJECT, // defines a field with an protocontract annotated object type
        FIELD_SIMPLE_OBJECT// defines a simple savebale object (class with some values in it using SaveableField annotations)
    }
}