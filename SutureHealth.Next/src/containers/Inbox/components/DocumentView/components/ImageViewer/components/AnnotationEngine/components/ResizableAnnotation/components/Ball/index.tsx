type BallProps = {
  color?: string;
};

const BallIcon = ({ color }: BallProps) => (
  <svg width="8" height="8" viewBox="0 0 8 8" fill="none" stroke={color}>
    <rect
      x="0.5"
      y="0.5"
      width="7"
      height="7"
      rx="3.5"
      fill="white"
      fillOpacity="1"
      stroke={color}
    />
  </svg>
);

export default BallIcon;
