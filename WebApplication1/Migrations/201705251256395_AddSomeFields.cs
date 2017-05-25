namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSomeFields : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Tests",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        Site_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Sites", t => t.Site_Id)
                .Index(t => t.Site_Id);
            
            AddColumn("dbo.Screenshots", "ScreenType", c => c.Int(nullable: false));
            AddColumn("dbo.Screenshots", "ScreenStatus", c => c.Int(nullable: false));
            AddColumn("dbo.Screenshots", "Test_Id", c => c.Int());
            CreateIndex("dbo.Screenshots", "Test_Id");
            AddForeignKey("dbo.Screenshots", "Test_Id", "dbo.Tests", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Tests", "Site_Id", "dbo.Sites");
            DropForeignKey("dbo.Screenshots", "Test_Id", "dbo.Tests");
            DropIndex("dbo.Tests", new[] { "Site_Id" });
            DropIndex("dbo.Screenshots", new[] { "Test_Id" });
            DropColumn("dbo.Screenshots", "Test_Id");
            DropColumn("dbo.Screenshots", "ScreenStatus");
            DropColumn("dbo.Screenshots", "ScreenType");
            DropTable("dbo.Tests");
        }
    }
}
