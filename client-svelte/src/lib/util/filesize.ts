/**
 * Formats a number of bytes into a human-readable string using SI units (base 1000)
 * @param bytes The number of bytes to format
 * @param decimals The number of decimal places to show (default: 2)
 * @returns Formatted string with appropriate SI unit
 */
export function formatFileSize(bytes: number, decimals: number = 2): string {
	if (bytes === 0) return '0 B';

	const base = 1024;
	const units = ['B', 'kB', 'MB', 'GB', 'TB', 'PB'];

	// Get the appropriate unit index by calculating log base 1000
	const i = Math.floor(Math.log(Math.abs(bytes)) / Math.log(base));

	// Format the number with the specified decimals
	const value = bytes / Math.pow(base, i);
	const formatted = i === 0 ? parseInt(value.toString()).toString() : value.toFixed(decimals);
	return `${formatted} ${units[i]}`;
}
