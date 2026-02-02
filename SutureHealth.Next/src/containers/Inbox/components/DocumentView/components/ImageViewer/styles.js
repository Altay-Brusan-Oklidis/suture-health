export default <style jsx global>{`
.save-fade-out {
  animation-name: 'save-fade-out';
  animation-duration: .6s;
}

@keyframes save-fade-out {
  0% {
    transform: scale(1);
  }

  25% {
    transform: scale(1.1);
  }

  100% {
    transform: scale(0);
  }
}
`}</style>
