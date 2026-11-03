const NICE_STEPS = [1, 2, 2.5, 5, 10]

export function niceTicks(max: number, maxIntervals = 5): number[] {
  if (!Number.isFinite(max) || max <= 0) return [0, 1]

  const magnitude = 10 ** Math.floor(Math.log10(max / maxIntervals))
  const step = NICE_STEPS.map((m) => m * magnitude).find((s) => Math.ceil(max / s) <= maxIntervals) ?? 10 * magnitude
  const intervals = Math.ceil(max / step)
  return Array.from({ length: intervals + 1 }, (_, i) => i * step)
}
