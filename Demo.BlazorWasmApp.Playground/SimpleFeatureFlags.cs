using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Demo.BlazorWasmApp.Playground
{
    public class SitemapRenderer
    {
        private readonly IConfiguration configuration;

        public SitemapRenderer(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string GetSitemap()
        {
            var allowIndexing = configuration.GetValue<bool>("AllowIndexing");

            // In a real application, this would generate a sitemap based on the application's routes and content
            return "<urlset><url><loc>https://example.com/</loc></url><url><loc>https://example.com/about</loc></url></urlset>";
        }
    }
}
