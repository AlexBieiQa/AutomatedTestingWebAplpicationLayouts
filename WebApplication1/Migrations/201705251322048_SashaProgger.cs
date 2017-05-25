namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SashaProgger : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Screenshots", "Date", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Screenshots", "Date", c => c.Int(nullable: false));
        }
    }
}
