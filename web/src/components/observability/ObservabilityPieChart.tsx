import { Cell, Legend, Pie, PieChart, ResponsiveContainer, Tooltip } from 'recharts';

export interface PieChartSlice {
  name: string;
  value: number;
  color?: string;
}

interface ObservabilityPieChartProps {
  data: PieChartSlice[];
  height?: number;
}

const defaultColors = ['#0d6efd', '#198754', '#dc3545', '#ffc107', '#6f42c1', '#20c997', '#fd7e14'];

export const ObservabilityPieChart = ({ data, height = 300 }: ObservabilityPieChartProps) => (
  <ResponsiveContainer width="100%" height={height}>
    <PieChart>
      <Pie
        data={data}
        dataKey="value"
        nameKey="name"
        cx="50%"
        cy="50%"
        outerRadius={100}
        label={({ name, percent }) => `${name} (${(percent * 100).toFixed(0)}%)`}
      >
        {data.map((entry, index) => (
          <Cell key={entry.name} fill={entry.color ?? defaultColors[index % defaultColors.length]} />
        ))}
      </Pie>
      <Tooltip />
      <Legend />
    </PieChart>
  </ResponsiveContainer>
);
