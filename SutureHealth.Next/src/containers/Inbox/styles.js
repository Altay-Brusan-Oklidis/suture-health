export default <style jsx global>{`
:root {
  --inbox-content-height: calc(100vh - var(--navbar-height));
  --filter-menu-width: 380px;
  --filter-sidebar-open-width: 234px;
  --filter-sidebar-close-width: 60px;
}

@media screen and (min-width: 320px) {
  :root {
    --document-actions-height: 96px;
  }
}

@media screen and (min-width: 768px){
  :root {
    --document-actions-height: 72px;
    --document-list-width: 46px;
  }
}

@media screen and (min-width: 1200px) {
  :root {
    --document-list-width: 396px;
  }
}
`}</style>
