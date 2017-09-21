SET QUOTED_IDENTIFIER ON
GO

--------------------------------------------------
-- ���[�����M��}�X�^
--------------------------------------------------
-- �\�폜
--DROP TABLE "mail_destination";
--go

-- �\�쐬
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

-- PK�쐬
ALTER TABLE "mail_destination" ADD CONSTRAINT "PK_mail_destination" PRIMARY KEY("mail_destination_id")
;
go

-- UNIQUE����쐬
ALTER TABLE "mail_destination" ADD CONSTRAINT "UQ_mail_destination_mail_address" UNIQUE("mail_address")
;
go

-- �R�����g�ݒ�
-- �e�[�u���R�����g
EXECUTE sp_addextendedproperty N'MS_Description', N'���[�����M��}�X�^', N'user', N'dbo', N'table', N'mail_destination';

-- �J�����R�����g
EXECUTE sp_addextendedproperty N'MS_Description', N'���[�����M��ID', N'user', N'dbo', N'table', N'mail_destination', N'column', N'mail_destination_id';
EXECUTE sp_addextendedproperty N'MS_Description', N'���[���A�h���X', N'user', N'dbo', N'table', N'mail_destination', N'column', N'mail_address';
EXECUTE sp_addextendedproperty N'MS_Description', N'����', N'user', N'dbo', N'table', N'mail_destination', N'column', N'description';
EXECUTE sp_addextendedproperty N'MS_Description', N'�\����', N'user', N'dbo', N'table', N'mail_destination', N'column', N'display_order';
EXECUTE sp_addextendedproperty N'MS_Description', N'�����t���O', N'user', N'dbo', N'table', N'mail_destination', N'column', N'is_disabled';
EXECUTE sp_addextendedproperty N'MS_Description', N'�o�^��', N'user', N'dbo', N'table', N'mail_destination', N'column', N'created_at';
EXECUTE sp_addextendedproperty N'MS_Description', N'�o�^��', N'user', N'dbo', N'table', N'mail_destination', N'column', N'created_user';
EXECUTE sp_addextendedproperty N'MS_Description', N'�X�V��', N'user', N'dbo', N'table', N'mail_destination', N'column', N'updated_at';
EXECUTE sp_addextendedproperty N'MS_Description', N'�X�V��', N'user', N'dbo', N'table', N'mail_destination', N'column', N'updated_user';
EXECUTE sp_addextendedproperty N'MS_Description', N'�o�[�W�����ԍ�', N'user', N'dbo', N'table', N'mail_destination', N'column', N'version_no';
