namespace CloudERP.Web.Models.Api
{
    public class GetProfileInfoResult : Result
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Role { get; set; }
    }
}