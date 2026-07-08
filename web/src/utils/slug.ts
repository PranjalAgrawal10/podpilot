export const createSlug = (name: string): string => {
  let slug = name.trim().toLowerCase();
  slug = slug.replace(/[^a-z0-9\s-]/g, '');
  slug = slug.replace(/\s+/g, '-');
  slug = slug.replace(/-+/g, '-');
  return slug.replace(/^-|-$/g, '');
};
