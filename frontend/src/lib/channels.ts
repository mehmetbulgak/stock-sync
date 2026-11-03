import type { ChannelType } from './api/types'

export const channelStyles: Record<ChannelType, { label: string; dot: string }> = {
  Hepsiburada: { label: 'Hepsiburada', dot: 'bg-[#2a78d6]' },
  Trendyol: { label: 'Trendyol', dot: 'bg-[#eb6834]' },
  WebStore: { label: 'Web Store', dot: 'bg-[#1baf7a]' },
  Manual: { label: 'Manual', dot: 'bg-[#eda100]' },
}
