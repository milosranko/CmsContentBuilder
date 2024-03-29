# CmsContentScaffolding
Scaffold thousands of pages with any structure using just a few lines of code,
Optimizely CMS and Piranha CMS supported

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
