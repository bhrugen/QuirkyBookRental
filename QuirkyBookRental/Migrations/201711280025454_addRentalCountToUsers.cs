namespace QuirkyBookRental.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addRentalCountToUsers : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "RentalCount", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "RentalCount");
        }
    }
}
