namespace CloudERP.Web.Models.Api
{
    public interface IResult
    {
        Response Code { get; set; }

        string Description { get; set; }
    }
}
