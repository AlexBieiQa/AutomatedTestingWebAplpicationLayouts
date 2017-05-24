namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserForSite : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sites", "User_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.Sites", "User_Id");
            AddForeignKey("dbo.Sites", "User_Id", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Sites", "User_Id", "dbo.AspNetUsers");
            DropIndex("dbo.Sites", new[] { "User_Id" });
            DropColumn("dbo.Sites", "User_Id");
        }
    }
}
