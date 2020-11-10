insert into permissions
(uuid, description, controller, module, createdAt, modifiedAt, status, isCustomizable, applyTo)
values(NEWID(), 'Mi SAT', 'SAT', 'CONFIGURATION', GETDATE(), GETDATE(), 'ACTIVE', 1, 'ONLY_ACCOUNT')

insert into permissions
(uuid, description, controller, module, createdAt, modifiedAt, status, isCustomizable, applyTo)
values(NEWID(), 'Roles', 'Role', 'CONFIGURATION', GETDATE(), GETDATE(), 'ACTIVE', 1, 'BOTH')

insert into permissions
(uuid, description, controller, module, createdAt, modifiedAt, status, isCustomizable, applyTo)
values(NEWID(), 'Usuarios', 'User', 'CONFIGURATION', GETDATE(), GETDATE(), 'ACTIVE', 1, 'BOTH')

insert into permissions
(uuid, description, controller, module, createdAt, modifiedAt, status, isCustomizable, applyTo)
values(NEWID(), 'Cuentas', 'Account', 'CONFIGURATION', GETDATE(), GETDATE(), 'ACTIVE', 0, 'BOTH')


insert into permissions
(uuid, description, controller, module, createdAt, modifiedAt, status, isCustomizable, applyTo)
values(NEWID(), 'Diagnóstico', 'Diagnostic', 'REPORTS', GETDATE(), GETDATE(), 'ACTIVE', 1, 'ONLY_ACCOUNT')

insert into permissions
(uuid, description, controller, module, createdAt, modifiedAt, status, isCustomizable, applyTo)
values(NEWID(), 'Clientes', 'Customer', 'CUSTOMERS', GETDATE(), GETDATE(), 'ACTIVE', 1, 'ONLY_ACCOUNT')

insert into permissions
(uuid, description, controller, module, createdAt, modifiedAt, status, isCustomizable, applyTo)
values(NEWID(), 'Proveedores', 'Provider', 'PROVIDERS', GETDATE(), GETDATE(), 'ACTIVE', 1, 'ONLY_ACCOUNT')

insert into permissions
(uuid, description, controller, module, createdAt, modifiedAt, status, isCustomizable, applyTo)
values(NEWID(), 'Banco', 'Bank', 'BANKS', GETDATE(), GETDATE(), 'ACTIVE', 1, 'ONLY_ACCOUNT')


insert into permissions
(uuid, description, controller, module, createdAt, modifiedAt, status, isCustomizable, applyTo)
values(NEWID(), 'Planes', 'Plan', 'PLANS', GETDATE(), GETDATE(), 'ACTIVE', 1, 'ONLY_BACK_OFFICE')

insert into permissions
(uuid, description, controller, module, createdAt, modifiedAt, status, isCustomizable, applyTo)
values(NEWID(), 'Alianzas y Descuentos', 'Alliance', 'ALLIANCES_DISCOUNTS', GETDATE(), GETDATE(), 'ACTIVE', 1, 'ONLY_BACK_OFFICE')

insert into permissions
(uuid, description, controller, module, createdAt, modifiedAt, status, isCustomizable, applyTo)
values(NEWID(), 'Promociones', 'Promotion', 'ALLIANCES_DISCOUNTS', GETDATE(), GETDATE(), 'ACTIVE', 1, 'ONLY_BACK_OFFICE')

insert into permissions
(uuid, description, controller, module, createdAt, modifiedAt, status, isCustomizable, applyTo)
values(NEWID(), 'Regularizaciones', 'Quotation', 'QUOTATION', GETDATE(), GETDATE(), 'ACTIVE', 1, 'ONLY_BACK_OFFICE')


insert into permissions
(uuid, description, controller, module, createdAt, modifiedAt, status, isCustomizable, applyTo)
values(NEWID(), 'Facturación', 'Invoicing', 'INVOICING', GETDATE(), GETDATE(), 'ACTIVE', 1, 'ONLY_ACCOUNT')


insert into permissions
(uuid, description, controller, module, createdAt, modifiedAt, status, isCustomizable, applyTo)
values(NEWID(), 'Sucursales', 'BranchOffice', 'CONFIGURATION', GETDATE(), GETDATE(), 'ACTIVE', 1, 'ONLY_ACCOUNT')


insert into roles
(uuid, name, code, description, createdAt, modifiedAt, status)
values(NEWID(), 'Account Owner', 'ACCOUNT_OWNER', 'ACCOUNT_OWNER', GETDATE(), GETDATE(), 'ACTIVE')


insert into roles
(uuid, name, code, description, createdAt, modifiedAt, status)
values(NEWID(), 'Lead', 'LEAD', 'LEAD', GETDATE(), GETDATE(), 'ACTIVE')


insert into roles
(uuid, name, code, description, createdAt, modifiedAt, status)
values(NEWID(), 'System Administrator', 'SYSTEM_ADMINISTRATOR', 'System Administrator', GETDATE(), GETDATE(), 'ACTIVE')


insert into rolesPermissions
(level, roleId, permissionId)
select 'FULL_ACCESS', 1, id from permissions
where applyTo<>'ONLY_BACK_OFFICE'

insert into rolesPermissions
(level, roleId, permissionId)
values ('FULL_ACCESS', 2, 4)

insert into rolesPermissions
(level, roleId, permissionId)
select 'FULL_ACCESS', 3, id from permissions

/*insert into users
(uuid, name, password, createdAt, modifiedAt, status, isBackOffice, profileId)
values (newid(), 'admin@admin.com', '3b612c75a7b5048a435fb6ec81e52ff92d6d795a8b5a9c17070f6a63c97a53b2', getdate(), getdate(), 'ACTIVE', 1,1)

insert into memberships
(userId, roleId,status)
values(19, 9, 'ACTIVE')

insert into rolesPermissions
(level, roleId, permissionId)
select 'FULL_ACCESS', 9, id from permissions*/

insert into planChargeConfigurations
(uuid, charge, createdAt, modifiedAt, status, planId, chargeId)
values(NEWID(), 20, GETDATE(), GETDATE(), 'ACTIVE', 1, 3)

insert into plans
(uuid, name, isCurrent, createdAt, modifiedAt, status)
values(NEWID(), 'Contigo', 1, GETDATE(), GETDATE(), 'ACTIVE')

insert into planCharges
(uuid, name, chargeType, createdAt, modifiedAt, status)
values(NEWID(), 'Contabilidad Mensual', 'FIXED', GETDATE(), GETDATE(), 'ACTIVE')


insert into planCharges
(uuid, name, chargeType, createdAt, modifiedAt, status, resourceTable, resourceField, operation)
values(NEWID(), 'Cargo por movimiento', 'VARIABLE', GETDATE(), GETDATE(), 'ACTIVE', 'movements', 'amount', 'MULTIPLICITY')

insert into planAssignments
(uuid, name, chargeType, resourceTable, resourceField, operation,dataType, fielType, providerData, createdAt, modifiedAt, status)
values(NEWID(), 'Rango de movimientos', 'VARIABLE', 'movements', 'amount', 'RANGE', 'System.Int32', 'text', null, GETDATE(), GETDATE(), 'ACTIVE')

insert into planAssignments
(uuid, name, chargeType, resourceTable, resourceField, operation,dataType, fielType, providerData, createdAt, modifiedAt, status)
values(NEWID(), 'Tipo de persona', 'VARIABLE', 'account', 'type', 'EQUAL', 'System.Int32', 'select', 'PersonType', GETDATE(), GETDATE(), 'ACTIVE')

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('09FC42BF-80EB-47B2-B7E6-5F608DFF3EA1', 'Normal (Simple Auth)', '56cf5728784806f72b8b4568', '2020-08-25 20:36:00.7766667', '2020-08-25 20:36:00.7766667', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('94F6A3AC-D312-4F10-B0DC-0DF6699F4304', 'Token (2FA)', '56cf5728784806f72b8b4569', '2020-08-25 20:36:00.7766667', '2020-08-25 20:36:00.7766667', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('CE053AF7-5811-414A-97E3-4317D9C5765B', 'Acme Error', '56cf5728784806f72b8b456a', '2020-08-25 20:36:00.7766667', '2020-08-25 20:36:00.7766667', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('D3BEF643-D4F1-4AE3-90E4-87F9CAD446F5', 'Captcha (2FA)', '572ba390784806060f8b458b', '2020-08-25 20:36:00.7766667', '2020-08-25 20:36:00.7766667', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('08EDD75D-E64C-49BA-AD03-D661FDA98C57', 'Multiple Image (2FA)', '573ce20978480645038b4589', '2020-08-25 20:36:00.7766667', '2020-08-25 20:36:00.7766667', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('87DEA9FF-B4FE-45A7-9C62-7580ED0EE689', 'Normal (case insensitive)', '58b884fc056f295aa1483a01', '2020-08-25 20:36:00.7766667', '2020-08-25 20:36:00.7766667', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('C9289EBA-73BD-415E-A777-CE78128FE89D', 'Multiple Text (2FA)', '58e28bce056f2903900d4d61', '2020-08-25 20:36:00.7766667', '2020-08-25 20:36:00.7766667', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('B8BCBDFF-B697-4845-A28D-75BDBECE1810', 'Normal with Attachments', '58b884fc056f295aa1483a02', '2020-08-25 20:36:00.7766667', '2020-08-25 20:36:00.7766667', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('9BE72E02-353A-44C2-8802-58596DDE0C98', 'Select Input (Simple Auth)', '5ae37e06056f290607126261', '2020-08-25 20:36:00.7766667', '2020-08-25 20:36:00.7766667', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('82E6726B-B1BD-45AC-A0BC-5EB05EDC5C7D', 'ACME Subcodes', '5c75cff1f9de2a0fae569281', '2020-08-25 20:36:00.7766667', '2020-08-25 20:36:00.7766667', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('1A943B5A-757D-414D-95ED-2D7DEBC0DFC3', 'Code Response Simulator (Simple Auth)', '5d0bb316f9de2a0a8f71aaf1', '2020-08-25 20:36:00.7766667', '2020-08-25 20:36:00.7766667', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('C49D89E1-C26D-4901-8875-8B769B53BBF3', 'CIEC (Sandbox)', '5da784f1f9de2a06483abec1', '2020-08-25 20:36:00.7766667', '2020-08-25 20:36:00.7766667', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('853C4941-E3F0-406A-B386-CA4E54721BE8', 'CIEC Retenciones (Sandbox)', '5f2c2aacd74b837fc10602c1', '2020-08-25 20:36:00.7766667', '2020-08-25 20:36:00.7766667', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('17A28971-49B1-4B73-805D-7E6E095F39B0', 'QR Code Scan (2FA)', '5efe27e84cc5f861897a5431', '2020-08-25 20:36:00.7766667', '2020-08-25 20:36:00.7766667', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('C13488D6-9B6C-4796-A695-6CC6C74F0A0B', 'Confirm Login (2FA)', '5f427f7810dff8730c50d1c1', '2020-08-25 20:36:00.7766667', '2020-08-25 20:36:00.7766667', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('AF0E8AEE-3B3E-4C59-A6CC-003A4FFC5ACC', 'Live Transactions', '5f427fae9c41a502e30ebe93', '2020-08-25 20:36:00.7766667', '2020-08-25 20:36:00.7766667', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('15EB3A91-D73A-46F1-91FA-BDF54D8A85D2', 'Disabled Transactions', '5f427fae9c41a502e30ebe94', '2020-08-25 20:36:00.7766667', '2020-08-25 20:36:00.7766667', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('C182E3AB-3B48-4F85-A777-38609AC0D053', 'Afirme', '56de2121784806a90a8b4586', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('D2D2D6EB-F3EF-411B-8E51-B44805B99B79', 'American Express', '572930c4784806060f8b456a', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('42075E36-1DC2-44E6-B68E-583D2BBB4CDF', 'Banco Azteca', '57292023784806af038b458a', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('A1AC563C-4951-412F-91D0-A5A4AFE87A70', 'Banamex', '56cf4ff5784806152c8b456a', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('D3FF32AC-8A40-4994-8214-2B3C9E3D75C3', 'BanBajio', '583db66378480635468b456a', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('66E35985-C0BE-45C6-B2D2-EC1D385F8CDE', 'BanCoppel', '57619d9278480678448b4567', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('97F57E66-FEB0-4413-A7C6-8EA4731F4CAD', 'Banorte IXE', '56cf4ff5784806152c8b456c', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('0A3D5E49-D704-4323-A249-22A2F94E1F9B', 'Banregio', '56cf4ff5784806152c8b456b', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('446646D6-1FE4-4BF9-A383-1BE9A74113CC', 'BBVA Bancomer', '56cf4ff5784806152c8b4569', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status)
values('6A748B0B-9222-45DE-82BE-FF99BE205043', 'HSBC', '5719a71a7848060f038b4567', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('7AB5727E-60E0-444C-86F2-4C58BF6FBED3', 'Inbursa', '574f16977848067c038b4592', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('A14681D2-70F9-4BF7-AF3D-6E5D70F57E27', 'Invex', '5760214578480648038b457d', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('C3D001D5-C1E7-4224-B2E2-2EB9210901A5', 'Liverpool', '57d1bccd784806b9488b456b', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('739F8D85-ECA9-4E84-9D0F-A4120CFC1566', 'Multiva', '57893e15784806f2218b457c', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('678D92C4-82ED-4C1F-834B-DDB3A481CDB9', 'Santander', '5731fb37784806a6118b4568', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('B5ABAC5F-3DE4-4A99-A170-1D3F66E510E1', 'Scotiabank', '5739cc3b7848066b028b4567', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('516420AD-3488-4951-BB03-D599E840C713', 'SAT', '56cf4ff5784806152c8b4568', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('9409F284-A845-4F02-814F-F99EC1C5921F', 'AT&T', '582f8a21784806eb348b4580', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('96182A10-F8DF-4965-B0EC-049A35D198D0', 'CFE', '57fed65178480609038b4586', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('54203E9F-2D6D-4FAC-BA94-2382A30089F3', 'Movistar', '582a05fb78480655628b45a0', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('5701C405-AB6A-475D-8F5F-ED0409E12309', 'Telcel', '580fa37978480600038b4570', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('23057E05-8067-43D4-A093-6FF0919C3EB5', 'Telmex', '57ffc01a7848060a038b45b0', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('C640053C-4E3E-471C-831B-1002779F825A', 'Banco Patagonia', '5941dd12056f29061d344ca0', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('287CE3C7-EF18-4916-A957-694E1A5EFED1', 'Banco de Chile', '5910b923056f2905ea05bbc1', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('C2178229-C858-4B57-9F85-ACB5BD68C2A0', 'Banco Ciudad', '5980bb94056f292f433f0cd0', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('B81E29A0-48E9-4E7A-80E1-780E1C63B61E', 'BBVA Francés', '59d2a397056f2925b252b981', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('14153E72-BAA1-4C62-942B-04CE99BD0951', 'Banco Credicoop', '5a3da41e056f2905c6245d60', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('5AC8BA47-C9DA-4BEC-A2FE-AAB8950A12C6', 'Coinbase', '5a29ccc0056f29062e404ec0', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('BC6579E8-8221-42CD-9335-131F6CDBE10E', 'Bitso', '5a309029056f2905c87b4e90', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('D0A0263B-2669-46EE-9E1C-E9E798AF5C62', 'BTC Blockchain', '5a85d00f056f290c504cf9f1', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('E006B333-36E3-40B4-B552-D73E6270ADCC', 'ETH Blockchain', '5a85d083056f290c4f19bdf1', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('049B86A3-EDD7-4A15-ACAD-A4CEDB24F8B6', 'Paypal', '5a85d146056f290c5117f1f1', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('68DD7998-D5B3-4CB8-B079-3AB5EAC67E6E', 'Raiffeisen Aval Bank', '5a98af66056f290595666a91', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('3822DAD9-39D1-45D9-B88C-A0309A99868C', 'Santander Rio', '5b0d8d1d056f2924ea7a2fb1', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('AD0DBD38-6696-45AD-B02E-3637F88DF527', 'Banco Popular', '5b21a6f7056f291e283d3161', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('88B392D7-8693-412F-9A10-BEFC4888F281', 'FirstBank', '5b329334056f2905fc7476a1', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('4007BC59-04BF-41B3-905C-5B4E8A8EEDBE', 'Banco Galicia', '5b566e0d7d8b6b0628564cf1', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('ED9A7159-86AA-4DF6-B46B-E4894AD0F3B9', 'UBS', '5b5b8eca7d8b6b06256fc5b1', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('0661E3CD-1666-4E9A-845D-0AA2FDC96FFF', 'Zurcher Kantonalbank', '5b5f68d97d8b6b0625599b11', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('3581EC75-3CDF-47A4-B8BF-173C2F46FBA5', 'Binance', '5bdce2cf7d8b6b412a5c6901', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('C7145179-02ED-44CF-866E-47FC3FBDD904', 'Bittrex', '5bdce4737d8b6b3b0b347ca1', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('E8771827-53BA-4879-8D5F-860DF3CBA156', 'Mistertango', '5bedae3a7d8b6b7db7365ad1', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('D30C95AB-EF59-493C-AD58-0B61AA5D69A1', 'NEO Blockchain', '5c085e50f9de2a7c2e38dd01', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('36BE56D9-D71D-488C-8AB5-23FFBB7A65D0', 'Credit Suisse', '5c098bdef9de2a0b2d4f14d1', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('442E2934-4EC8-4A94-88B6-6B2A32C96740', 'Turicum Private Bank', '5c09c6bef9de2a142e264581', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('D8CE74E0-E28F-45B2-AC65-BB3921195D98', 'Bank Frick', '5c099a40f9de2a0eaf6ef851', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('12EC6D7A-0738-4A65-8A58-84224B8555F2', 'EOS Blockchain', '5c0ec03cf9de2a0aab61d021', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('EA268338-C659-4C33-A38E-1471884DC7B6', 'Banco Macro', '5c23bc89f9de2a1a022f0e51', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('E3BADAC4-C729-44E2-94C3-422BEE7FA01E', 'ICBC', '5c3f82a4f9de2a08177b2d41', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('EEC7F256-1441-4C99-885B-FB23A819EA62', 'AFIP', '5c65e306f9de2a080b61bb31', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('A645293E-F452-491F-BF4D-BC1996D8E05A', 'Banco Comafi', '5c7b6035f9de2a087b355c81', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('0AEB84DF-455F-4924-A3B5-B494282E0DA2', 'Vexi', '5cacd760f9de2a09df4b3e61', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('A4CD0EA8-76CF-4C22-BC88-BE6924B92D57', 'Bx+', '5d07afd1f9de2a7a6119aee1', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('BF181CB1-7A22-4AA8-83D3-F2A0948E6530', 'Coinmetro', '5d1a4bd6f9de2a2453227271', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('B216A921-82E2-4301-9F0C-6577D37C1979', 'Banco Provincia', '5d1ce9a6f9de2a07ed574491', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('82EB6B94-B9D5-4796-9A6C-12DDB36921B3', 'Banco Supervielle', '5d430ba4f9de2a07fb58d611', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('F0980171-9015-416B-AEEC-FBF4ED2D79C4', 'INE', '5d5588fdf9de2a0cf05399a1', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('AD6F4DE6-9909-45D6-843F-99EE0D174D17', 'Renapo', '5d4b57e7f9de2a0ad215fba1', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('0185CE74-3124-48B3-996C-C480B9E6761E', 'Banco Nacion', '5d77c3fbf9de2a08f33d9031', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('2AB25D74-5369-4269-A0EB-479C51B67DE0', 'Banco Caja Social', '5dc06e5af9de2a098f7e2fa1', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('87EE1190-BF86-40C6-B635-E5EEC125B689', 'N26', '5dc06939f9de2a084655b3e1', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('80D1F84F-7E56-45A4-984E-3A3F932DAB33', 'Bancolombia', '5dcb3d6ff9de2a197966d861', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('885861ED-29F2-46D1-9FBC-4035477E0774', 'Scotiabank', '5dcaf01bf9de2a08f66eafc1', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('D67DE65B-2250-4434-AD3F-1800FD9815DE', 'BAC Credomatic Costa Rica', '5ddeddca754bb96e7c76bf51', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('4E38B546-6077-4C5C-9DDE-67090FF27122', 'Banco de Cordoba', '5e2626c89be72331726b8501', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('13B3EDD7-E80C-42D1-8452-B6642A50EC99', 'BAC Credomatic El Salvador', '5e2896623557f42193381f31', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('CDB99970-9657-4CCC-A78C-82FDEC198628', 'Autofin', '5e8619eaebcdd053f523f521', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('9496E252-D000-4F87-A227-84EBB868FE5F', 'CIBanco', '5e83eb11873d81650375eaa1', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('1C160749-BB12-40F2-BF8B-0989AD748926', 'Davivienda', '5eb589ee087dd67473246d71', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('13BB4DC2-FE35-4D56-82D6-F21DBFAD0BCA', 'Banco Mercantil Santa Cruz', '5f11e3ddcf8ce559206859a1', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE') 

insert into banks (uuid, name, providerId, createdAt, modifiedAt, status) 
values('F9BA4AF2-0C55-411C-99A8-39A02915E140', 'Banco Económico', '5f11e3f0bff67e6c9b0b6b01', '2020-10-08 17:48:56.3200000', '2020-10-08 17:48:56.3200000', 'ACTIVE')
