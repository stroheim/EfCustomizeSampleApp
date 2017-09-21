SET QUOTED_IDENTIFIER ON
GO

--------------------------------------------------
-- メール送信先マスタ
--------------------------------------------------
-- 表削除
--DROP TABLE "mail_destination";
--go

-- 表作成
CREATE TABLE "mail_destination"
(
 "mail_destination_id"	INT				IDENTITY															NOT NULL
,"mail_address"			VARCHAR(50)																			NOT NULL
,"description"			VARCHAR(255)		
,"display_order"		INT																					NOT NULL
,"is_disabled"			BIT																					NOT NULL
,"created_at"			DATETIME2		CONSTRAINT "DF_mail_destination_created_at" DEFAULT GETDATE()		NOT NULL
,"created_user"			VARCHAR(100)																		NOT NULL
,"updated_at"			DATETIME2		
,"updated_user"			VARCHAR(100)		
,"version_no"			ROWVERSION																			NOT NULL
);
go

-- PK作成
ALTER TABLE "mail_destination" ADD CONSTRAINT "PK_mail_destination" PRIMARY KEY("mail_destination_id")
;
go

-- UNIQUE制約作成
ALTER TABLE "mail_destination" ADD CONSTRAINT "UQ_mail_destination_mail_address" UNIQUE("mail_address")
;
go

-- コメント設定
-- テーブルコメント
EXECUTE sp_addextendedproperty N'MS_Description', N'メール送信先マスタ', N'user', N'dbo', N'table', N'mail_destination';

-- カラムコメント
EXECUTE sp_addextendedproperty N'MS_Description', N'メール送信先ID', N'user', N'dbo', N'table', N'mail_destination', N'column', N'mail_destination_id';
EXECUTE sp_addextendedproperty N'MS_Description', N'メールアドレス', N'user', N'dbo', N'table', N'mail_destination', N'column', N'mail_address';
EXECUTE sp_addextendedproperty N'MS_Description', N'説明', N'user', N'dbo', N'table', N'mail_destination', N'column', N'description';
EXECUTE sp_addextendedproperty N'MS_Description', N'表示順', N'user', N'dbo', N'table', N'mail_destination', N'column', N'display_order';
EXECUTE sp_addextendedproperty N'MS_Description', N'無効フラグ', N'user', N'dbo', N'table', N'mail_destination', N'column', N'is_disabled';
EXECUTE sp_addextendedproperty N'MS_Description', N'登録日', N'user', N'dbo', N'table', N'mail_destination', N'column', N'created_at';
EXECUTE sp_addextendedproperty N'MS_Description', N'登録者', N'user', N'dbo', N'table', N'mail_destination', N'column', N'created_user';
EXECUTE sp_addextendedproperty N'MS_Description', N'更新日', N'user', N'dbo', N'table', N'mail_destination', N'column', N'updated_at';
EXECUTE sp_addextendedproperty N'MS_Description', N'更新者', N'user', N'dbo', N'table', N'mail_destination', N'column', N'updated_user';
EXECUTE sp_addextendedproperty N'MS_Description', N'バージョン番号', N'user', N'dbo', N'table', N'mail_destination', N'column', N'version_no';
