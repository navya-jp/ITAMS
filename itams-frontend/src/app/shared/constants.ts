// Dropdown constants for the application

export const INDIAN_STATES = [
  'Andhra Pradesh', 'Arunachal Pradesh', 'Assam', 'Bihar', 'Chhattisgarh',
  'Goa', 'Gujarat', 'Haryana', 'Himachal Pradesh', 'Jharkhand', 'Karnataka',
  'Kerala', 'Madhya Pradesh', 'Maharashtra', 'Manipur', 'Meghalaya', 'Mizoram',
  'Nagaland', 'Odisha', 'Punjab', 'Rajasthan', 'Sikkim', 'Tamil Nadu',
  'Telangana', 'Tripura', 'Uttar Pradesh', 'Uttarakhand', 'West Bengal',
  'Andaman and Nicobar Islands', 'Chandigarh', 'Dadra and Nagar Haveli and Daman and Diu',
  'Delhi', 'Jammu and Kashmir', 'Ladakh', 'Lakshadweep', 'Puducherry'
];

export const SPV_NAMES = [
  'NHAI (National Highways Authority of India)',
  'NHIDCL (National Highways & Infrastructure Development Corporation)',
  'IHMCL (India Highway Management Company Limited)',
  'MSRDC (Maharashtra State Road Development Corporation)',
  'GMSRDC (Gujarat State Road Development Corporation)',
  'KRIDL (Karnataka Road Development Corporation Limited)',
  'TNRDC (Tamil Nadu Road Development Company)',
  'UPRNN (Uttar Pradesh Rajya Nirman Nigam)',
  'WBHIDCO (West Bengal Housing Infrastructure Development Corporation)',
  'APIIC (Andhra Pradesh Industrial Infrastructure Corporation)',
  'HRIDC (Haryana Rail Infrastructure Development Corporation)',
  'PMIDC (Punjab Maritime Infrastructure Development Corporation)'
];

export const DISTRICTS_BY_STATE: { [key: string]: string[] } = {
  'Maharashtra': [
    'Ahmednagar', 'Akola', 'Amravati', 'Aurangabad', 'Beed', 'Bhandara', 'Buldhana',
    'Chandrapur', 'Dhule', 'Gadchiroli', 'Gondia', 'Hingoli', 'Jalgaon', 'Jalna',
    'Kolhapur', 'Latur', 'Mumbai City', 'Mumbai Suburban', 'Nagpur', 'Nanded',
    'Nandurbar', 'Nashik', 'Osmanabad', 'Palghar', 'Parbhani', 'Pune', 'Raigad',
    'Ratnagiri', 'Sangli', 'Satara', 'Sindhudurg', 'Solapur', 'Thane', 'Wardha',
    'Washim', 'Yavatmal'
  ],
  'Gujarat': [
    'Ahmedabad', 'Amreli', 'Anand', 'Aravalli', 'Banaskantha', 'Bharuch', 'Bhavnagar',
    'Botad', 'Chhota Udaipur', 'Dahod', 'Dang', 'Devbhoomi Dwarka', 'Gandhinagar',
    'Gir Somnath', 'Jamnagar', 'Junagadh', 'Kachchh', 'Kheda', 'Mahisagar', 'Mehsana',
    'Morbi', 'Narmada', 'Navsari', 'Panchmahal', 'Patan', 'Porbandar', 'Rajkot',
    'Sabarkantha', 'Surat', 'Surendranagar', 'Tapi', 'Vadodara', 'Valsad'
  ],
  // Add more states as needed
  'Karnataka': [
    'Bagalkot', 'Ballari', 'Belagavi', 'Bengaluru Rural', 'Bengaluru Urban', 'Bidar',
    'Chamarajanagar', 'Chikballapur', 'Chikkamagaluru', 'Chitradurga', 'Dakshina Kannada',
    'Davanagere', 'Dharwad', 'Gadag', 'Hassan', 'Haveri', 'Kalaburagi', 'Kodagu',
    'Kolar', 'Koppal', 'Mandya', 'Mysuru', 'Raichur', 'Ramanagara', 'Shivamogga',
    'Tumakuru', 'Udupi', 'Uttara Kannada', 'Vijayapura', 'Yadgir'
  ]
};

export const PLAZA_NAMES = [
  'Main Plaza', 'Entry Plaza', 'Exit Plaza', 'Toll Plaza A', 'Toll Plaza B',
  'Junction Plaza', 'Service Plaza', 'Rest Area Plaza', 'Fuel Plaza', 'Food Plaza'
];

export const GOVERNMENT_CODES = [
  'NH-1', 'NH-2', 'NH-3', 'NH-4', 'NH-5', 'NH-6', 'NH-7', 'NH-8', 'NH-9', 'NH-10',
  'NH-11', 'NH-12', 'NH-13', 'NH-14', 'NH-15', 'NH-16', 'NH-17', 'NH-18', 'NH-19', 'NH-20',
  'SH-1', 'SH-2', 'SH-3', 'SH-4', 'SH-5', 'MDR-1', 'MDR-2', 'MDR-3', 'ODR-1', 'ODR-2'
];

export const OFFICE_NAMES = [
  'Head Office', 'Regional Office', 'District Office', 'Sub-District Office',
  'Project Office', 'Site Office', 'Administrative Office', 'Technical Office',
  'Finance Office', 'HR Office', 'Operations Office', 'Maintenance Office'
];

export const INTERNAL_LOCATIONS = [
  'DG Room', 'Admin Building', 'SWB', 'Electrical Room', 'Server Room',
  'Control Room', 'Cashier Room', 'Tunnel', 'Security Cabin', 'Parking Area',
  'Canteen', 'Rest Room', 'Store Room', 'Workshop', 'Fuel Station'
];

export const LATITUDE_RANGES = {
  min: 6.0, // Southernmost point of India
  max: 37.6 // Northernmost point of India
};

export const LONGITUDE_RANGES = {
  min: 68.0, // Westernmost point of India
  max: 97.25 // Easternmost point of India
};

export const LANE_OPTIONS = Array.from({ length: 20 }, (_, i) => i + 1);