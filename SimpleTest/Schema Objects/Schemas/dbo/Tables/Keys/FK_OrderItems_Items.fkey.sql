ALTER TABLE [dbo].[OrderItems]
    ADD CONSTRAINT [FK_OrderItems_Items] FOREIGN KEY ([ItemId]) REFERENCES [dbo].[Items] ([ItemId]) ON DELETE NO ACTION ON UPDATE NO ACTION;

