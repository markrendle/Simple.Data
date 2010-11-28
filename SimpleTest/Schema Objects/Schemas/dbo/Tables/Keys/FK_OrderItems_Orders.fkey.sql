ALTER TABLE [dbo].[OrderItems]
    ADD CONSTRAINT [FK_OrderItems_Orders] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders] ([OrderId]) ON DELETE NO ACTION ON UPDATE NO ACTION;

