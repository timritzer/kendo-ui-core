require 'rbconfig'

README_DIR = 'resources'
THIRD_PARTY_LEGAL_DIR = File.join('resources', 'legal', 'third-party')
LESSC = File.join(Rake.application.original_dir, "build", "less-js", "bin", "lessc")
CSSMIN = File.join(Rake.application.original_dir, "node_modules", "cssmin", "bin", "cssmin")
COMPILEJS = File.join(Rake.application.original_dir, "build", "compile.js");
UGLIFYJS = File.join(Rake.application.original_dir, "node_modules", "uglify-js", "bin", "uglifyjs");

class MergeTask < Rake::FileTask
    def execute(args=nil)
        File.open(name, 'w') do |output|
            dependencies = prerequisites.find_all{|file| !/build\/.*\.rb$/i.match(file)}
            $stderr.puts "Merge\n\t#{dependencies.join("\n\t")} \nto #{name}" if VERBOSE

            dependencies.each do |src|
                File.open(src, 'r:bom|utf-8') do |file|
                    output.write file.read
                end
            end
        end
    end
end

class LicenseTask < Rake::FileTask
    def execute(args=nil)
        ensure_path(name)

        template = File.read(prerequisites[0]).gsub(/\r\n/, "\n")

        File.open(name, "w") do |file|
            file.write(template.sub("#= version #", VERSION).sub("#= year #", Time.now.year.to_s))
        end
    end

    def needed?
        super || !File.read(name).include?(VERSION)
    end
end

def ensure_path(path)
    dir = path.pathmap('%d')
    mkdir_p dir, :verbose => false unless Dir.exists?(dir)
end

def file_merge(*args, &block)
    MergeTask.define_task(*args, &block)
end

def file_license(*args, &block)
    LicenseTask.define_task(*args, &block)
end

def subject_to_license?(file)
    file.pathmap('%f') =~ /^kendo(.+)(js|css|less)$/
end

def msbuild(project, options=nil)
    options = '/p:Configuration=Release' if options == nil

    msbuild_path = 'c:\\Windows\\Microsoft.NET\\Framework64\\v4.0.30319\\msbuild.exe'

    msbuild_path = 'xbuild' if PLATFORM =~ /linux|darwin/

    sh "#{msbuild_path} /v:q #{project} #{options}", :verbose => VERBOSE
end

def mvn(name, options)
    cmd = 'mvn '
    cmd = cmd + '-q ' unless VERBOSE

    sh "#{cmd}-f #{name} #{options}", :verbose => VERBOSE
end

def uglifyjs(from, to)
    cmd = "node #{UGLIFYJS} #{from} -o #{to} -cm"
    cmd = cmd + " warnings=false" unless VERBOSE
    sh cmd, :verbose => VERBOSE
end

def compilejs(from, options=nil)
    cmd = "node #{COMPILEJS} #{from} #{options}"
    sh cmd, :verbose => VERBOSE
end

def compilejs_bundle(t)
    deps = t.prerequisites.join(" ")
    file = t.name
    cmd = "node #{COMPILEJS} #{file} --bundle #{deps}"
    sh cmd, :verbose => VERBOSE
end

def less(from, to)
    sh "node #{LESSC} #{from} #{to}", :verbose => VERBOSE
end

def cssmin(from, to)
    sh "node #{CSSMIN} #{from} > #{to}", :verbose => VERBOSE
end

# Copy file when it is modified
def file_copy(options)
    to = options[:to]
    license = options[:license]
    from = options[:from]

    if license && subject_to_license?(to)
        prerequisites = [from, license]
    else
        prerequisites = from
    end

    file to => prerequisites do |t|
        ensure_path to

        if license && subject_to_license?(to)
            $stderr.puts "cp #{from} #{to}" if VERBOSE

            File.open(from, 'r:bom|utf-8') do |source|
                contents = source.read

                File.open(to, "w") do |file|
                    file.write(File.read(license))
                    contents.sub!("$KENDO_VERSION", VERSION)
                    file.write(contents)
                end
            end
        else
            cp from, to, :verbose => VERBOSE
        end
    end
end

def tree(options)
    dir = options[:to]
    root = options[:root]

    source = FileList[*options[:from]].reject { |f| File.directory? f }

    destination = source.sub(root, "#{dir}/")

    file dir => destination

    destination.each_with_index do |f, index|
        src = source[index]

        file_copy :to => f, :from => src, :license => options[:license]
    end
end

