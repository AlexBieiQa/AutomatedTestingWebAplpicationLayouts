namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddScreenBase64 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Screenshots", "ScreenBase64", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Screenshots", "ScreenBase64");
        }
    }
}
