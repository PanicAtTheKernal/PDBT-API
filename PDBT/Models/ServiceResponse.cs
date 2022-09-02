using Microsoft.AspNetCore.Mvc;

namespace PDBT.Models;

public class ServiceResponse<T>
{
    public T? Data { get; set; }
    public bool Success { get; set; } = true;
    public ActionResult<T> Result { get; set; } = null!;
}