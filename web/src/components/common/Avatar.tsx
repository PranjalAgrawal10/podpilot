interface AvatarProps {
  name: string;
  size?: 'sm' | 'md' | 'lg';
  className?: string;
}

const sizeMap = {
  sm: 32,
  md: 40,
  lg: 56,
};

const getInitials = (name: string): string => {
  const parts = name.trim().split(/\s+/).filter(Boolean);
  if (parts.length === 0) return '?';
  if (parts.length === 1) return parts[0].charAt(0).toUpperCase();
  return `${parts[0].charAt(0)}${parts[parts.length - 1].charAt(0)}`.toUpperCase();
};

export const Avatar = ({ name, size = 'md', className = '' }: AvatarProps) => {
  const dimension = sizeMap[size];

  return (
    <span
      className={`avatar avatar-${size} ${className}`.trim()}
      style={{ width: dimension, height: dimension, fontSize: dimension * 0.4 }}
      title={name}
      aria-label={name}
    >
      {getInitials(name)}
    </span>
  );
};
