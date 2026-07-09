import {
  Area,
  AreaChart,
  CartesianGrid,
  Legend,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';
import type { ChartSeries } from './ObservabilityLineChart';

interface ObservabilityAreaChartProps {
  data: Record<string, string | number>[];
  xKey: string;
  series: ChartSeries[];
  height?: number;
  yAxisLabel?: string;
  stacked?: boolean;
}

const defaultColors = ['#0d6efd', '#198754', '#dc3545', '#ffc107', '#6f42c1'];

export const ObservabilityAreaChart = ({
  data,
  xKey,
  series,
  height = 300,
  yAxisLabel,
  stacked = false,
}: ObservabilityAreaChartProps) => (
  <ResponsiveContainer width="100%" height={height}>
    <AreaChart data={data} margin={{ top: 8, right: 16, left: 0, bottom: 0 }}>
      <CartesianGrid strokeDasharray="3 3" stroke="rgba(128,128,128,0.2)" />
      <XAxis dataKey={xKey} tick={{ fontSize: 12 }} />
      <YAxis tick={{ fontSize: 12 }} label={yAxisLabel ? { value: yAxisLabel, angle: -90, position: 'insideLeft' } : undefined} />
      <Tooltip />
      <Legend />
      {series.map((item, index) => (
        <Area
          key={item.key}
          type="monotone"
          dataKey={item.key}
          name={item.name}
          stroke={item.color ?? defaultColors[index % defaultColors.length]}
          fill={item.color ?? defaultColors[index % defaultColors.length]}
          fillOpacity={0.2}
          stackId={stacked ? 'stack' : undefined}
        />
      ))}
    </AreaChart>
  </ResponsiveContainer>
);
