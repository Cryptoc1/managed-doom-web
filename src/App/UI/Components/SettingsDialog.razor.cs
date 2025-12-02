using System.ComponentModel.DataAnnotations;

namespace ManagedDoom.App.UI.Components;

internal sealed record class SettingsForm
{
    [Required]
    public string? Value { get; set; }
}