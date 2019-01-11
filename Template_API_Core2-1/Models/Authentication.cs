namespace Template_API_Core2_1.Models
{
    public class Authentication
    {
        /// <summary>
        /// This model should be changed to match the requirements of the security
        /// Token that is to be generated, either adding additional schema values
        /// or free text values that will be stored as part of the Claim
        /// </summary>
        
        // TODO:// Alter Model as Required for use, below is an example
        
        public string EmailAddress { get; set; } = string.Empty;
        public int PrimaryId { get; set; } = -1;
        public int PrimaryGroupId { get; set; } = -1;
        public int RoleId { get; set; } = -1;
    }
}
