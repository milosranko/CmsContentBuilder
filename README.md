# CmsContentScaffolding
Scaffold thousands of pages with any structure using just a few lines of code,
Optimizely CMS and Piranha CMS supported
```
    builder.UseCmsContentBuilder(
      builderOptions: new CmsContentApplicationBuilderOptions
      {
          DefaultLanguage = "sr",
          BuildMode = BuildModeEnum.OnlyIfEmpty
      },
      builder: contentBuilder =>
      {
          contentBuilder.WithPage<StartPage>(page =>
          {
              page.Name = "StartPage";
              page.MainContentArea
              .AddBlock<TeaserBlock>()
              .AddBlock<TeaserBlock>(block =>
              {
                  block.Heading = PropertyHelpers.AddRandomText();
                  block.LeadText = PropertyHelpers.AddRandomText(150);
              });
          }, level1 =>
          {
              level1.WithPage<ArticlePage>(page =>
              {
                  page.Name = "Article1_1";
              }, level2 =>
              {
                  level2.WithPage<ArticlePage>(page =>
                  {
                      page.Name = "Article2_1";
                  });
                  level2.WithPage<ArticlePage>(options: level3 =>
                  {
                      level3.WithPages<ArticlePage>(totalPages: 20);
                  });
              });
              level1.WithPages<ArticlePage>(totalPages: 100);
          });
      });
```
Also you can scaffold Optimizely Commerce catalog structure
```
app.UseCmsContentScaffolding(
    builderOptions: o =>
    {
        o.SiteName = "Demo";
        o.Language = CultureInfo.GetCultureInfo("en");
        o.SiteHost = "https://localhost:5000";
        o.BuildMode = BuildMode.Append;
        o.StartPageType = typeof(StartPage);
    },
    builder: b =>
    {
        b.UseAssets(referenceConverter.GetRootLink())
        .WithContent<CatalogContent>(x =>
        {
            x.Name = "Catalog 1";
            x.DefaultCurrency = "EUR";
            x.DefaultLanguage = "en";
            x.WeightBase = "kgs";
            x.LengthBase = "cm";
        }, l1 => l1.WithContent<FashionNode>(x => x.Name = "Men", l2 =>
                    l2.WithContent<FashionNode>(x => x.Name = "Shoes", l3 =>
                        l3.WithContent<FashionProduct>(x => x.Name = "Product 1", l4 =>
                            l4
                            .WithContent<FashionVariant>(v => v.Name = "Variant 1")
                            .WithContent<FashionVariant>(v => v.Name = "Variant 2"))
                        .WithContent<FashionProduct>(x => x.Name = "Product 2")
                        .WithContent<FashionProduct>(x => x.Name = "Product 3")
                 ).WithContent<FashionNode>(x => x.Name = "Accessories", l3 =>
                    l3
                    .WithContent<FashionProduct>(x => x.Name = "Product 1")
                    .WithContent<FashionProduct>(x => x.Name = "Product 2")
                    .WithContent<FashionProduct>(x => x.Name = "Product 3")
                 )
            )
        );
    });
```
