﻿using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web;
using Optimizely.Demo.PublicWeb.Models.Blocks;
using Optimizely.Demo.PublicWeb.Models.Pages.Base;
using System.ComponentModel.DataAnnotations;

namespace Optimizely.Demo.PublicWeb.Models.Pages;

[ContentType(
	GUID = "{E6F693A7-1436-49CC-B7F0-D7555D530DD6}",
	GroupName = "MetaData")]
[AvailableContentTypes(
	Availability.Specific,
	Include = new[]
	{
		typeof(NotFoundPage),
		typeof(ArticlePage)
	})]
public class StartPage : PageBaseSeo
{
	#region Settings tab

	[Display(
		GroupName = SystemTabNames.Settings,
		Order = 100)]
	public virtual string SiteName { get; set; }

	[Display(
		GroupName = SystemTabNames.Settings,
		Order = 110)]
	[AllowedTypes(typeof(ArticlePage))]
	public virtual ContentReference MainArticlePageReference { get; set; }

	[Display(
		GroupName = SystemTabNames.Settings,
		Order = 120)]
	[AllowedTypes(typeof(NotFoundPage))]
	public virtual ContentReference NotFoundPageReference { get; set; }

	#endregion

	#region Content tab

	[CultureSpecific]
	[Display(
			GroupName = SystemTabNames.Content,
			Order = 100)]
	public virtual string Heading { get; set; }

	[CultureSpecific]
	[Display(
		GroupName = SystemTabNames.Content,
		Order = 110)]
	[UIHint(UIHint.Textarea, PresentationLayer.Edit)]
	public virtual string LeadText { get; set; }

	[CultureSpecific]
	[Display(
			GroupName = SystemTabNames.Content,
			Order = 120)]
	[AllowedTypes(new[] {
			typeof(TeaserBlock)
		})]
	public virtual ContentArea MainContentArea { get; set; }

	#endregion

	#region Public properties

	public override void SetDefaultValues(ContentType contentType)
	{
		base.SetDefaultValues(contentType);

		SiteName = "DEMO";
	}

	#endregion
}
