namespace SutureHealth.Hchb
{
    public static class Messages
    {
        public const string ADMIT_SUCCESS = "Admit the patient successfully.";
        public const string DISCHARGE_SUCCESS = "Discharge the patient successfully.";
        public const string UPDATE_SUCCESS = "Update the patient successfully.";
        public const string CREATE_SUCCESS = "Create the patient successfully.";
        public const string CANCLE_SUCCESS = "Cancle the admission successfully.";

        public const string PATIENT_EXIST = "The patient already exists.";
        public const string PATIENT_NONMATCH = "There is no matched patient.";

        public const string ADMITED_PATIENT = "Patient is already admitted.";

        public const string DISCHARGED_PATIENT = "Patient is already discharged.";
        public const string DISCHARGE_CANCLED_PATIENT = "Can't discharge cancled patient.";

        public const string CANCLE_NONADMITED_PATIENT = "Patient is already cancled.";

        public const string INVALID_STATUS = "Patient has invalid status.";
        public const string INVALID_TYPE = "The message type is invalid.";

        public const string DOCUMENT_SEND = "Send document successfully.";

        public const string ADT_ERROR = "Error processing ADT message";

        public const string SENDER_NOTEXIST_ERROR = "Sender's facility does not exist";
        public const string SIGNER_NOTEXIST_ERROR = "Signer does not exist";
        public const string SIGNER_FACILITY_NOTEXIST_ERROR = "Signer's facility does not exist";
        public const string TEMPLATE_NOTEXIST_ERROR = "There is no matched template.";
        public const string NOT_VALID_HCHB_DOCUMENT_ERROR = "This is not the document from HCHB";

    }
}
