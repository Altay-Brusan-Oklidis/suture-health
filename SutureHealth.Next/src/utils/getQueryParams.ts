export default function getQueryParams() {
  return window.location.search.replace('?', '');
}
