namespace QuirkyBookRental.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBookRentalToDatabaseCheck : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.BookRents", "UserId", c => c.String(nullable: false));
            AlterColumn("dbo.BookRents", "RentalDuration", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.BookRents", "RentalDuration", c => c.String());
            AlterColumn("dbo.BookRents", "UserId", c => c.String());
        }
    }
}
