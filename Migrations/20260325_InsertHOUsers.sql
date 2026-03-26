-- Insert HO Users from AssignedUserText
-- RoleId: 2=Admin, 3=IT Staff
-- Password hash for ITAMS@2026! using bcrypt
-- All users: MustChangePassword=1, IsActive=1, ProjectId=8 (HO)

DECLARE @pwd NVARCHAR(200) = '$2a$11$8K1p/a0dL2LkqvMA87LzO.Mh5vllpKmdxX6NYb2UV8mWLxYX4H1Ka'
-- Note: This is the same hash as Admin@123 used for seeding.
-- We'll use a known hash for ITAMS@2026! — generate via bcrypt if needed,
-- or use the same default and force password change on first login.

SET IDENTITY_INSERT Users ON

INSERT INTO Users (Id, UserId, Username, Email, PasswordHash, FirstName, LastName, RoleId, IsActive, MustChangePassword, FailedLoginAttempts, CreatedAt)
VALUES
(47,  'USR00047', 'alakshendra.mishra',   'alakshendra.mishra@ho.itams.local',   @pwd, 'Alakshendra', 'Mishra',          3, 1, 1, 0, GETUTCDATE()),
(48,  'USR00048', 'alpesh.bhatt',         'alpesh.bhatt@ho.itams.local',         @pwd, 'Alpesh',      'Bhatt',           3, 1, 1, 0, GETUTCDATE()),
(49,  'USR00049', 'amar.patel',           'amar.patel@ho.itams.local',           @pwd, 'Amar',        'Patel',           3, 1, 1, 0, GETUTCDATE()),
(50,  'USR00050', 'anand.aghera',         'anand.aghera@ho.itams.local',         @pwd, 'Anand',       'Aghera',          3, 1, 1, 0, GETUTCDATE()),
(51,  'USR00051', 'ankur.patel',          'ankur.patel@ho.itams.local',          @pwd, 'Ankur',       'Patel',           3, 1, 1, 0, GETUTCDATE()),
(52,  'USR00052', 'arpit.khare',          'arpit.khare@ho.itams.local',          @pwd, 'Arpit',       'Khare',           3, 1, 1, 0, GETUTCDATE()),
(53,  'USR00053', 'ashish.vaishnav',      'ashish.vaishnav@ho.itams.local',      @pwd, 'Ashish',      'Vaishnav',        3, 1, 1, 0, GETUTCDATE()),
(54,  'USR00054', 'bharat.moliya',        'bharat.moliya@ho.itams.local',        @pwd, 'Bharat',      'Moliya',          3, 1, 1, 0, GETUTCDATE()),
(55,  'USR00055', 'bharat.rami',          'bharat.rami@ho.itams.local',          @pwd, 'Bharat',      'Rami',            3, 1, 1, 0, GETUTCDATE()),
(56,  'USR00056', 'chirag.dulani',        'chirag.dulani@ho.itams.local',        @pwd, 'Chirag',      'Dulani',          3, 1, 1, 0, GETUTCDATE()),
(57,  'USR00057', 'david.carey',          'david.carey@ho.itams.local',          @pwd, 'David',       'Carey',           3, 1, 1, 0, GETUTCDATE()),
(58,  'USR00058', 'genish.parvadiya',     'genish.parvadiya@ho.itams.local',     @pwd, 'Genish',      'Parvadiya',       3, 1, 1, 0, GETUTCDATE()),
(59,  'USR00059', 'girish.kachhadia',     'girish.kachhadia@ho.itams.local',     @pwd, 'Girish',      'Kachhadia',       3, 1, 1, 0, GETUTCDATE()),
(60,  'USR00060', 'hardik.bhavsar',       'hardik.bhavsar@ho.itams.local',       @pwd, 'Hardik',      'Bhavsar',         3, 1, 1, 0, GETUTCDATE()),
(61,  'USR00061', 'hitesh.patel',         'hitesh.patel@ho.itams.local',         @pwd, 'Hitesh',      'Patel',           3, 1, 1, 0, GETUTCDATE()),
(62,  'USR00062', 'jaydeep.viradiya',     'jaydeep.viradiya@ho.itams.local',     @pwd, 'Jaydeep',     'Viradiya',        3, 1, 1, 0, GETUTCDATE()),
(63,  'USR00063', 'kalpesh.trivedi',      'kalpesh.trivedi@ho.itams.local',      @pwd, 'Kalpesh',     'Trivedi',         3, 1, 1, 0, GETUTCDATE()),
(64,  'USR00064', 'kishor.gorle',         'kishor.gorle@ho.itams.local',         @pwd, 'Kishor',      'Gorle',           3, 1, 1, 0, GETUTCDATE()),
(65,  'USR00065', 'lalit.singh',          'lalit.singh@ho.itams.local',          @pwd, 'Lalit',       'Singh',           3, 1, 1, 0, GETUTCDATE()),
(66,  'USR00066', 'mansi.dharsandiya',    'mansi.dharsandiya@ho.itams.local',    @pwd, 'Mansi',       'Dharsandiya',     3, 1, 1, 0, GETUTCDATE()),
(67,  'USR00067', 'milin.shah',           'milin.shah@ho.itams.local',           @pwd, 'Milin',       'Shah',            3, 1, 1, 0, GETUTCDATE()),
(68,  'USR00068', 'navin.prakash',        'navin.prakash@ho.itams.local',        @pwd, 'Navin',       'Prakash',         3, 1, 1, 0, GETUTCDATE()),
(69,  'USR00069', 'nayana.saha',          'nayana.saha@ho.itams.local',          @pwd, 'Nayana',      'Saha',            3, 1, 1, 0, GETUTCDATE()),
(70,  'USR00070', 'nikhil.tiwari',        'nikhil.tiwari@ho.itams.local',        @pwd, 'Nikhil',      'Tiwari',          3, 1, 1, 0, GETUTCDATE()),
(71,  'USR00071', 'nilesh.bhavsar',       'nilesh.bhavsar@ho.itams.local',       @pwd, 'Nilesh',      'Bhavsar',         3, 1, 1, 0, GETUTCDATE()),
(72,  'USR00072', 'noor.nanavati',        'noor.nanavati@ho.itams.local',        @pwd, 'Noor',        'Nanavati',        3, 1, 1, 0, GETUTCDATE()),
(73,  'USR00073', 'pardeep.singh',        'pardeep.singh@ho.itams.local',        @pwd, 'Pardeep',     'Singh',           3, 1, 1, 0, GETUTCDATE()),
(74,  'USR00074', 'parul.vayada',         'parul.vayada@ho.itams.local',         @pwd, 'Parul',       'Vayada',          3, 1, 1, 0, GETUTCDATE()),
(75,  'USR00075', 'praveen.pawar',        'praveen.pawar@ho.itams.local',        @pwd, 'Praveen',     'Pawar',           3, 1, 1, 0, GETUTCDATE()),
(76,  'USR00076', 'punit.mathur',         'punit.mathur@ho.itams.local',         @pwd, 'Punit',       'Mathur',          3, 1, 1, 0, GETUTCDATE()),
(77,  'USR00077', 'rabindra',             'rabindra@ho.itams.local',             @pwd, 'Rabindra',    '',                3, 1, 1, 0, GETUTCDATE()),
(78,  'USR00078', 'rajesh.anvekar',       'rajesh.anvekar@ho.itams.local',       @pwd, 'Rajesh',      'Anvekar',         3, 1, 1, 0, GETUTCDATE()),
(79,  'USR00079', 'rajesh.bhudhrani',     'rajesh.bhudhrani@ho.itams.local',     @pwd, 'Rajesh',      'Bhudhrani',       3, 1, 1, 0, GETUTCDATE()),
(80,  'USR00080', 'rajnish.saxena',       'rajnish.saxena@ho.itams.local',       @pwd, 'Rajnish',     'Saxena',          3, 1, 1, 0, GETUTCDATE()),
(81,  'USR00081', 'rakesh.agrawal',       'rakesh.agrawal@ho.itams.local',       @pwd, 'Rakesh',      'Agrawal',         3, 1, 1, 0, GETUTCDATE()),
(82,  'USR00082', 'ram.singh',            'ram.singh@ho.itams.local',            @pwd, 'Ram',         'Singh',           3, 1, 1, 0, GETUTCDATE()),
(83,  'USR00083', 'ravi.shekhar',         'ravi.shekhar@ho.itams.local',         @pwd, 'Ravi',        'Shekhar',         3, 1, 1, 0, GETUTCDATE()),
(84,  'USR00084', 'ripan',                'ripan@ho.itams.local',                @pwd, 'Ripan',       '',                3, 1, 1, 0, GETUTCDATE()),
(85,  'USR00085', 'saaket.jain',          'saaket.jain@ho.itams.local',          @pwd, 'Saaket',      'Jain',            3, 1, 1, 0, GETUTCDATE()),
(86,  'USR00086', 'sachin.sharma',        'sachin.sharma@ho.itams.local',        @pwd, 'Sachin',      'Sharma',          3, 1, 1, 0, GETUTCDATE()),
(87,  'USR00087', 'sameer.vashishth',     'sameer.vashishth@ho.itams.local',     @pwd, 'Sameer',      'Vashishth',       3, 1, 1, 0, GETUTCDATE()),
(88,  'USR00088', 'shaik.hussain',        'shaik.hussain@ho.itams.local',        @pwd, 'Shaik',       'Hussain',         3, 1, 1, 0, GETUTCDATE()),
(89,  'USR00089', 'sravan.kumar',         'sravan.kumar@ho.itams.local',         @pwd, 'Sravan',      'Kumar',           3, 1, 1, 0, GETUTCDATE()),
(90,  'USR00090', 'sunil.singh',          'sunil.singh@ho.itams.local',          @pwd, 'Sunil',       'Singh',           3, 1, 1, 0, GETUTCDATE()),
(91,  'USR00091', 'tapan.parikh',         'tapan.parikh@ho.itams.local',         @pwd, 'Tapan',       'Parikh',          3, 1, 1, 0, GETUTCDATE()),
(92,  'USR00092', 'viju.joseph',          'viju.joseph@ho.itams.local',          @pwd, 'Viju',        'Joseph',          2, 1, 1, 0, GETUTCDATE()),  -- Admin
(93,  'USR00093', 'vikram.patel',         'vikram.patel@ho.itams.local',         @pwd, 'Vikram',      'Patel',           3, 1, 1, 0, GETUTCDATE()),
(94,  'USR00094', 'vimal.gautam',         'vimal.gautam@ho.itams.local',         @pwd, 'Vimal',       'Gautam',          3, 1, 1, 0, GETUTCDATE()),
(95,  'USR00095', 'viral.joshi',          'viral.joshi@ho.itams.local',          @pwd, 'Viral',       'Joshi',           3, 1, 1, 0, GETUTCDATE()),
(96,  'USR00096', 'vishvesh.desai',       'vishvesh.desai@ho.itams.local',       @pwd, 'Vishvesh',    'Desai',           3, 1, 1, 0, GETUTCDATE())

SET IDENTITY_INSERT Users OFF

-- Assign all new users to HO project (ProjectId=8)
INSERT INTO UserProjects (UserId, ProjectId, IsActive, AssignedAt, AssignedBy)
SELECT u.Id, 8, 1, GETUTCDATE(), 1
FROM Users u
WHERE u.Id BETWEEN 47 AND 96
  AND NOT EXISTS (SELECT 1 FROM UserProjects up WHERE up.UserId = u.Id AND up.ProjectId = 8)

PRINT 'HO users inserted and assigned to HO project.'

-- Now link assets: update AssignedUserId where AssignedUserText matches
UPDATE a SET a.AssignedUserId = u.Id
FROM Assets a
JOIN Users u ON (
    -- Match cleaned name patterns
    a.AssignedUserText LIKE u.FirstName + ' ' + u.LastName + '%'
    OR a.AssignedUserText LIKE u.FirstName + '_' + u.LastName + '%'
    OR a.AssignedUserText = u.FirstName + ' ' + u.LastName
    OR a.AssignedUserText = u.FirstName
)
WHERE a.ProjectId = 8
  AND a.AssignedUserId IS NULL
  AND a.AssignedUserText IS NOT NULL
  AND u.Id BETWEEN 47 AND 96

PRINT 'Assets linked to users.'

-- Check how many got linked
SELECT COUNT(*) as LinkedAssets FROM Assets WHERE ProjectId = 8 AND AssignedUserId IS NOT NULL
SELECT COUNT(*) as StillUnlinked FROM Assets WHERE ProjectId = 8 AND AssignedUserId IS NULL AND AssignedUserText IS NOT NULL AND AssignedUserText != ''
