
using Microsoft.AspNetCore.Razor.Language;

namespace RazorTextTemplating.PreProcessor
{
    internal sealed class SuppressMetadataAttributesFeature : IConfigureRazorCodeGenerationOptionsFeature
    {
        public int Order => 10;
        public RazorEngine Engine { get; set; }

        public void Configure(RazorCodeGenerationOptionsBuilder options)
        {
            if (null != options)
            {
                options.SuppressChecksum = true;
                options.SuppressMetadataAttributes = true;
            }
        }
    }
}
