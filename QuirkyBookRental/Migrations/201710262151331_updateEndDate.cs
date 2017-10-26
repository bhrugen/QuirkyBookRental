namespace QuirkyBookRental.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateEndDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BookRents", "ScheduledEndDate", c => c.DateTime());
            AddColumn("dbo.BookRents", "ActualEndDate", c => c.DateTime());
            DropColumn("dbo.BookRents", "ScheduledStartDate");
            DropColumn("dbo.BookRents", "ActualStartDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.BookRents", "ActualStartDate", c => c.DateTime());
            AddColumn("dbo.BookRents", "ScheduledStartDate", c => c.DateTime());
            DropColumn("dbo.BookRents", "ActualEndDate");
            DropColumn("dbo.BookRents", "ScheduledEndDate");
        }
    }
}
