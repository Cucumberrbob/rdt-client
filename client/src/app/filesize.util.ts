export function filesize(value: number) {
  if (typeof value !== 'number' || isNaN(value) || value < 0) {
    throw new TypeError('Invalid number');
  }

  const UNITS = ['B', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];

  const exponent = Math.min(Math.floor(Math.log(value) / Math.log(1000)), UNITS.length - 1);
  const result = value / Math.pow(1000, exponent);
  return `${result.toFixed(2)} ${UNITS[exponent]}`;
}
