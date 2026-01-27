import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ValidationService {

  validateProjectCode(code: string): { valid: boolean; message?: string } {
    if (!code) {
      return { valid: false, message: 'Project code is required' };
    }
    
    if (code.length < 3 || code.length > 20) {
      return { valid: false, message: 'Project code must be 3-20 characters' };
    }
    
    if (!/^[A-Z0-9_-]+$/.test(code)) {
      return { valid: false, message: 'Only uppercase letters, numbers, hyphens, and underscores allowed' };
    }
    
    return { valid: true };
  }

  validatePlazaCode(code: string): { valid: boolean; message?: string } {
    if (!code) {
      return { valid: false, message: 'Plaza code is required' };
    }
    
    if (code.length < 2 || code.length > 15) {
      return { valid: false, message: 'Plaza code must be 2-15 characters' };
    }
    
    if (!/^[A-Z0-9_-]+$/.test(code)) {
      return { valid: false, message: 'Only uppercase letters, numbers, hyphens, and underscores allowed' };
    }
    
    return { valid: true };
  }

  validateChainageNumber(chainage: string): { valid: boolean; message?: string } {
    if (!chainage) {
      return { valid: false, message: 'Chainage number is required' };
    }
    
    if (!/^\d{3}\.\d{3}$/.test(chainage)) {
      return { valid: false, message: 'Format must be 000.000' };
    }
    
    return { valid: true };
  }

  validateCoordinates(lat: number, lng: number): { valid: boolean; message?: string } {
    if (lat < 6.0 || lat > 37.6) {
      return { valid: false, message: 'Latitude must be between 6.0 and 37.6 (India bounds)' };
    }
    
    if (lng < 68.0 || lng > 97.25) {
      return { valid: false, message: 'Longitude must be between 68.0 and 97.25 (India bounds)' };
    }
    
    return { valid: true };
  }

  formatChainageNumber(input: string): string {
    // Remove all non-digits
    const digits = input.replace(/\D/g, '');
    
    // Limit to 6 digits
    const limited = digits.substring(0, 6);
    
    // Format as 000.000
    if (limited.length === 0) {
      return '';
    } else if (limited.length <= 3) {
      return limited;
    } else {
      const first = limited.substring(0, 3);
      const second = limited.substring(3);
      return `${first}.${second}`;
    }
  }

  generateProjectCode(spvName: string, states: string[]): string {
    // Extract abbreviation from SPV name
    let spvCode = '';
    
    // Check if SPV name starts with an abbreviation (like "NHAI (National...)")
    if (spvName.includes('(')) {
      spvCode = spvName.split('(')[0].trim().toUpperCase();
    } else {
      // Take first 4 characters of each word
      spvCode = spvName.split(' ')
        .map(word => word.charAt(0))
        .join('')
        .substring(0, 4)
        .toUpperCase();
    }
    
    // Create state code from first 2 letters of each state
    const stateCode = states.map(state => state.substring(0, 2).toUpperCase()).join('');
    
    // Use last 2 digits of timestamp for uniqueness
    const timestamp = Date.now().toString().slice(-2);
    
    return `${spvCode}_${stateCode}_${timestamp}`;
  }

  generatePlazaCode(plazaName: string, projectCode: string): string {
    const plazaCode = plazaName.replace(/\s+/g, '').substring(0, 4).toUpperCase();
    const projectSuffix = projectCode.split('_').pop() || '0000';
    
    return `PLZ_${plazaCode}_${projectSuffix}`;
  }
}