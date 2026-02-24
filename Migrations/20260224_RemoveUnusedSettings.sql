-- Migration: Remove AllowMultipleSessions and SessionWarningMinutes settings
-- Date: 2026-02-24
-- Description: Remove unused system settings that are no longer needed

-- Remove AllowMultipleSessions setting
DELETE FROM SystemSettings WHERE SettingKey = 'AllowMultipleSessions';

-- Remove SessionWarningMinutes setting
DELETE FROM SystemSettings WHERE SettingKey = 'SessionWarningMinutes';

PRINT 'Removed AllowMultipleSessions and SessionWarningMinutes settings';
