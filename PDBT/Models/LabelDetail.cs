using System.ComponentModel.DataAnnotations;

namespace PDBT.Models;

public class LabelDetail
{
    public int IssueId { get; set; }
    public int LabelId { get; set; }
    public Issue? Issue { get; set; }
    public Label? Label { get; set; }
}