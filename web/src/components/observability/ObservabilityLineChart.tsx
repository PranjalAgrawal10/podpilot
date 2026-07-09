import {
  CartesianGrid,
  Legend,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';

export interface ChartSeries {
  key: string;
  name: string;
  color?: string;
  unit?: string;
}

interface ObservabilityLineChartProps {
  data: Record<string, string | number>[];
  xKey: string;
  series: ChartSeries[];
  height?: number;
  yAxisLabel?: string;
}

const defaultColors = ['#0d6efd', '#198754', '#dc3545', '#ffc107', '#6f42c1'];

export const ObservabilityLineChart = ({
  data,
  xKey,
  series,
  height = 300,
  yAxisLabel,
}: ObservabilityLineChartProps) => (
  <ResponsiveContainer width="100%" height={height}>
    <LineChart data={data} margin={{ top: 8, right: 16, left: 0, bottom: 0 }}>
      <CartesianGrid strokeDasharray="3 3" stroke="rgba(128,128,128,0.2)" />
      <XAxis dataKey={xKey} tick={{ fontSize: 12 }} />
      <YAxis tick={{ fontSize: 12 }} label={yAxisLabel ? { value: yAxisLabel, angle: -90, position: 'insideLeft' } : undefined} />
      <Tooltip
        formatter={(value: number, name: string) => {
          const seriesConfig = series.find((item) => item.name === name);
          return seriesConfig?.unit ? [`${value}${seriesConfig.unit}`, name] : [value, name];
        }}
      />
      <Legend />
      {series.map((item, index) => (
        <Line
          key={item.key}
          type="monotone"
          dataKey={item.key}
          name={item.name}
          stroke={item.color ?? defaultColors[index % defaultColors.length]}
          strokeWidth={2}
          dot={false}
          activeDot={{ r: 4 }}
        />
      ))}
    </LineChart>
  </ResponsiveContainer>
);
