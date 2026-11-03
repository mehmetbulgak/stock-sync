const TIME_ZONE = 'Europe/Istanbul'

const currency = new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY', maximumFractionDigits: 2 })

const compactCurrency = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'TRY',
  currencyDisplay: 'narrowSymbol',
  notation: 'compact',
  minimumFractionDigits: 0,
  maximumFractionDigits: 1,
})

const number = new Intl.NumberFormat('en-US')

const dateTime = new Intl.DateTimeFormat('en-GB', {
  timeZone: TIME_ZONE,
  day: '2-digit',
  month: 'short',
  hour: '2-digit',
  minute: '2-digit',
})

const shortDay = new Intl.DateTimeFormat('en-GB', { timeZone: 'UTC', weekday: 'short', day: 'numeric' })

export const formatCurrency = (value: number) => currency.format(value)

export const formatCompactCurrency = (value: number) => compactCurrency.format(value)

export const formatNumber = (value: number) => number.format(value)

export const formatDateTime = (iso: string) => dateTime.format(new Date(iso))

export const formatDay = (isoDate: string) => shortDay.format(new Date(`${isoDate}T00:00:00Z`))

export function pluralize(count: number, singular: string, plural = `${singular}s`) {
  return `${formatNumber(count)} ${count === 1 ? singular : plural}`
}

export function formatShare(part: number, total: number) {
  if (total === 0 || part === 0) return '0%'
  const percent = (part / total) * 100
  return percent < 1 ? '<1%' : `${Math.round(percent)}%`
}
