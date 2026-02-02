import Image from 'next/image';
import illustrationImage from '@public/images/InboxMarketingIllustration.png';
import yourLogoHere from '@public/images/YourLogoHere.png';

interface Props {
  logoSrc?: string;
}

export default function Illustration({ logoSrc }: Props) {
  const logoWidth = 50;
  const logoHeight = 50;

  return (
    <div className="container">
      <Image src={illustrationImage} alt="" width="564" height="295" />
      <div className="logo_img">
        {logoSrc ? (
          <img src={logoSrc} alt="" width={logoWidth} height={logoHeight} />
        ) : (
          <Image
            src={yourLogoHere}
            alt=""
            width={logoWidth}
            height={logoHeight}
          />
        )}
      </div>
      <style jsx>{`
        .container {
          position: relative;
        }
        .logo_img {
          position: absolute;
          top: 10px;
          right: 20px;
        }
      `}</style>
    </div>
  );
}
