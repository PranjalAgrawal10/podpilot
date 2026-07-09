import {
  Bar,
  BarChart,
  CartesianGrid,
  Legend,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';
import type { ChartSeries } from './ObservabilityLineChart';

interface ObservabilityBarChartProps {
  data: Record<string, string | number>[];
  xKey: string;
  series: ChartSeries[];
  height?: number;
  yAxisLabel?: string;
}

const defaultColors = ['#0d6efd', '#198754', '#dc3545', '#ffc107', '#6f42c1'];

export const ObservabilityBarChart = ({
  data,
  xKey,
  series,
  height = 300,
  yAxisLabel,
}: ObservabilityBarChartProps) => (
  <ResponsiveContainer width="100%" height={height}>
    <BarChart data={data} margin={{ top: 8, right: 16, left: 0, bottom: 0 }}>
      <CartesianGrid strokeDasharray="3 3" stroke="rgba(128,128,128,0.2)" />
      <XAxis dataKey={xKey} tick={{ fontSize: 12 }} />
      <YAxis tick={{ fontSize: 12 }} label={yAxisLabel ? { value: yAxisLabel, angle: -90, position: 'insideLeft' } : undefined} />
      <Tooltip />
      <Legend />
      {series.map((item, index) => (
        <Bar
          key={item.key}
          dataKey={item.key}
          name={item.name}
          fill={item.color ?? defaultColors[index % defaultColors.length]}
        />
      ))}
    </BarChart>
  </ResponsiveContainer>
);
