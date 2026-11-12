using UnityEditor;

// v0.0.53 B09: the save path read .../unknown/unknown because the company was left as Unity's default.
// Application.persistentDataPath = <base>/<companyName>/<productName>, so set both once on editor load.
namespace Hellord.EditorTools
{
    [InitializeOnLoad]
    static class HellordPlayerSettings53
    {
        static HellordPlayerSettings53()
        {
            if (string.IsNullOrWhiteSpace(PlayerSettings.companyName) || PlayerSettings.companyName == "DefaultCompany") PlayerSettings.companyName = "Hellord";
            if (string.IsNullOrWhiteSpace(PlayerSettings.productName)) PlayerSettings.productName = "Hellord";
        }
    }
}
