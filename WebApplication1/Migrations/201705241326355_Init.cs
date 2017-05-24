namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Links",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ValueUrl = c.String(),
                        Screenshot_Id = c.Int(),
                        Site_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Screenshots", t => t.Screenshot_Id)
                .ForeignKey("dbo.Sites", t => t.Site_Id)
                .Index(t => t.Screenshot_Id)
                .Index(t => t.Site_Id);
            
            CreateTable(
                "dbo.Screenshots",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ImgUrl = c.String(),
                        Date = c.Int(nullable: false),
                        Site_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Sites", t => t.Site_Id)
                .Index(t => t.Site_Id);
            
            CreateTable(
                "dbo.Sites",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Url = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Links", "Site_Id", "dbo.Sites");
            DropForeignKey("dbo.Links", "Screenshot_Id", "dbo.Screenshots");
            DropForeignKey("dbo.Screenshots", "Site_Id", "dbo.Sites");
            DropIndex("dbo.Screenshots", new[] { "Site_Id" });
            DropIndex("dbo.Links", new[] { "Site_Id" });
            DropIndex("dbo.Links", new[] { "Screenshot_Id" });
            DropTable("dbo.Sites");
            DropTable("dbo.Screenshots");
            DropTable("dbo.Links");
        }
    }
}
