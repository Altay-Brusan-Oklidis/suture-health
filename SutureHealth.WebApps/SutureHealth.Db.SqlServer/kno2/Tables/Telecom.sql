CREATE TABLE [kno2].[Telecom]
(
    [PatientId] BIGINT NOT NULL, 
    [System] VARCHAR(5) NULL, -- 0 = Phone, 1 = Fax, 2 = Email, 3 = Pager, 4 = Url, 5 = Sms, 6 = Other
    [Use] VARCHAR(4) NULL, -- 0 = H, 1 = Hp, 2 = Hv, 3 = Wp, 4 = Dir, 5 = Pub, 6 = Tmp, 7 = Old, 8 = Bad, 9 = Conf, 10 = Phys, 11 = Pst, 12 = As, 13 = Ec, 14 = Pg, 15 = Mc
    [Value] VARCHAR(320) NOT NULL,
    CONSTRAINT [FK_Telecom_Patient] FOREIGN KEY ([PatientId]) REFERENCES [kno2].[Patient](Id)
)
